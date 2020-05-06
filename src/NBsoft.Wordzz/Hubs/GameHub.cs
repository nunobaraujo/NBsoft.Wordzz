using Microsoft.AspNetCore.SignalR;
using NBsoft.Wordzz.Contracts;
using NBsoft.Wordzz.Contracts.Entities;
using NBsoft.Wordzz.Contracts.Requests;
using NBsoft.Wordzz.Contracts.Results;
using NBsoft.Wordzz.Core.Models;
using NBsoft.Wordzz.Core.Services;
using NBsoft.Wordzz.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NBsoft.Wordzz.Hubs
{
    public class GameHub : Hub
    {
        public const string Address = "/hubs/game";

        private readonly IGameService gameService;

        public GameHub(IGameService gameService)
        {
            this.gameService = gameService;
        }
                
        public async override Task OnConnectedAsync()
        {   
            var user = Context.User?.Identities.FirstOrDefault()?.Name;
            ClientHandler.AddClient(new WordzzClient {
                ConnectionDate = DateTime.UtcNow,
                ConnectionId = Context.ConnectionId,
                UserName = user
            });
            await Clients.Others.SendAsync("connected", user);
            await base.OnConnectedAsync();
        }
        public async override Task OnDisconnectedAsync(Exception exception)
        {
            var user = ClientHandler.Find(Context.ConnectionId);            
            ClientHandler.RemoveClient(Context.ConnectionId);
            await Clients.Others.SendAsync("disconnected", user.UserName);
            await base.OnDisconnectedAsync(exception);
        }
        
        public async Task<IEnumerable<string>> GetOnlineContacts() 
        {
            var client = ClientHandler.Find(Context.ConnectionId);
            if (client == null)
                return null;
            var allContacts = await gameService.GetContacts(client.UserName);
            var onlineContacts = ClientHandler.Clients
                .Where(c => allContacts.Contains(c.UserName))
                .Select(c => c.UserName)
                .Distinct();
            return onlineContacts;
        }
        public Task<IEnumerable<string>> GetOnlineOpponents()
        {
            var client = ClientHandler.Find(Context.ConnectionId);
            var gameOpponents = gameService.GetActiveGameOpponents(client.UserName);

            var onlineContacts = ClientHandler.Clients
                .Where(c => gameOpponents.Contains(c.UserName))
                .Select(c => c.UserName)
                .Distinct();
            return Task.FromResult(onlineContacts);
        }
        public Task<IEnumerable<IGame>> GetActiveGames()
        {   
            var client = ClientHandler.Find(Context.ConnectionId);
            var games = gameService.GetActiveGames(client.UserName);
            return Task.FromResult(games);
        }
        public Task<IEnumerable<IGameChallenge>> GetReceivedChallenges()
        {
            var player = ClientHandler.Find(Context.ConnectionId);
            if (player == null)
            {
                IEnumerable<IGameChallenge> empty = new List<IGameChallenge>();
                return Task.FromResult(empty);
            }   

            var challenges = gameService.GetReceivedChallenges(player.UserName);
            return Task.FromResult(challenges);
        }
        public Task<IEnumerable<IGameChallenge>> GetSentChallenges()
        {
            var player = ClientHandler.Find(Context.ConnectionId);
            if (player == null)
            {
                IEnumerable<IGameChallenge> empty = new List<IGameChallenge>();
                return Task.FromResult(empty);
            }

            var challenges = gameService.GetSentChallenges(player.UserName);
            return Task.FromResult(challenges);
        }

        public Task<IGameQueue> SearchGame(string language ="en-us", string boardName = "Standard")
        {
            var user = ClientHandler.Find(Context.ConnectionId);
            if (user == null || user.UserName == null)
                return null;

            var result = gameService.SearchGame(user.UserName, language, boardName);
            return Task.FromResult(result);
        }
        public async Task<IGameChallenge> ChallengeGame(string language , string challengedPlayer, int size = 15)
        {
            if (string.IsNullOrEmpty(language))
                language = "en-us";
            
            var challenger = ClientHandler.Find(Context.ConnectionId);
            var opposer = ClientHandler.FindByUserName(challengedPlayer);

            if (opposer == null)
            {
                return null;
            }
            var challenge = gameService.ChallengeGame(language, challenger.UserName, opposer.UserName, size);            
            await SendChallengePlayer(opposer?.ConnectionId, challenge);
            return challenge;
        }
        public async Task<string> ChallengeAccept(string queueId, bool accept)
        {
            if (string.IsNullOrEmpty(queueId))
                return null;

            var challengedPlayer = ClientHandler.Find(Context.ConnectionId);
            var queue = gameService.GetQueuedGame(queueId);
            var game = await gameService.AcceptChallenge(challengedPlayer.UserName, queueId, accept);
            if (queue == null){
                return null;
            }
            var challengerConnection = ClientHandler.FindByUserName(queue.Player1);            
            await SendChallengeAccepted(challengerConnection?.ConnectionId, queue.Id, accept, game?.Id);
            return game?.Id;
        }

        public async Task<PlayResult> Play(PlayRequest playRequest) 
        {
            var game = gameService.GetActiveGames(playRequest.UserName).Single(g => g.Id == playRequest.GameId);
            var opponent = playRequest.UserName == game.Player01.UserName
                ? game.Player02.UserName
                : game.Player01.UserName;
            var opponentConnection = ClientHandler.FindByUserName(opponent);

            var result = await gameService.Play(playRequest.GameId, playRequest.UserName, playRequest.Letters);
            if (result.MoveResult == "OK")
            {
                
                await SendPlayOk(opponentConnection?.ConnectionId, playRequest.GameId, playRequest.UserName);
            }
            return result;
        }
        public async Task<PlayResult> Pass(PlayRequest playRequest)
        {
            var game = gameService.GetActiveGames(playRequest.UserName).Single(g => g.Id == playRequest.GameId);
            var opponent = playRequest.UserName == game.Player01.UserName
                ? game.Player02.UserName
                : game.Player01.UserName;
            var opponentConnection = ClientHandler.FindByUserName(opponent);

            var result = await gameService.Pass(playRequest.GameId, playRequest.UserName);
            if (result.MoveResult == "OK" || result.MoveResult == "GameOver")
            {                
                if (result.MoveResult == "OK")
                    await SendPlayOk(opponentConnection?.ConnectionId, playRequest.GameId, playRequest.UserName);
                else
                    await SendGameOver(opponentConnection?.ConnectionId, playRequest.GameId, result.GameOverResult);
            }
            return result;
        }
        public async Task<PlayResult> Forfeit(PlayRequest playRequest)
        {
            var game = gameService.GetActiveGames(playRequest.UserName).Single(g => g.Id == playRequest.GameId);
            var opponent = playRequest.UserName == game.Player01.UserName
                ? game.Player02.UserName
                : game.Player01.UserName;
            var opponentConnection = ClientHandler.FindByUserName(opponent);

            var result = await gameService.Forfeit(playRequest.GameId, playRequest.UserName);
            if (result.MoveResult == "GameOver")
            {   
                await SendGameOver(opponentConnection?.ConnectionId, playRequest.GameId, result.GameOverResult);
            }
            return result;
        }

        public async Task SendToAll(string name, string message)
        {
            await Clients.All.SendAsync("sendToAll", name, message);
        }
        private async Task SendPlayOk(string connectionId, string gameId, string username)
        {
            if (!string.IsNullOrEmpty(connectionId))
                await Clients.Client(connectionId).SendAsync("playOk", gameId, username);
        }
        private async Task SendChallengePlayer(string connectionId, IGameChallenge challenge)
        {
            if (!string.IsNullOrEmpty(connectionId))
                await Clients.Client(connectionId).SendAsync("newChallenge", challenge);
        }
        private async Task SendChallengeAccepted(string connectionId, string challengeId, bool accept, string gameId)
        {
            if (!string.IsNullOrEmpty(connectionId))
                await Clients.Client(connectionId).SendAsync("challengeAccepted", challengeId, accept, gameId);
        }      
        private async Task SendGameOver(string connectionId, string gameId, GameResult result)
        {
            if (!string.IsNullOrEmpty(connectionId))
                await Clients.Client(connectionId).SendAsync("gameOver", gameId, result);
        }
    }
}
