using Microsoft.AspNetCore.SignalR;
using NBsoft.Logs;
using NBsoft.Logs.Interfaces;
using NBsoft.Wordzz.Contracts;
using NBsoft.Wordzz.Contracts.Requests;
using NBsoft.Wordzz.Contracts.Results;
using NBsoft.Wordzz.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NBsoft.Wordzz.Hubs
{
    public class GameHub : Hub<IGameClient>
    {
        public const string Address = "/hubs/game";

        private readonly IGameService gameService;
        private readonly ILogger logger;

        public GameHub(IGameService gameService, ILogger logger)
        {
            this.gameService = gameService;
            this.logger = logger;
        }

        public async override Task OnConnectedAsync()
        {
            var user = Context.User?.Identities.FirstOrDefault()?.Name;
            if (user != null)
            {
                ClientHandler.AddClient(new WordzzClient
                {
                    ConnectionDate = DateTime.UtcNow,
                    ConnectionId = Context.ConnectionId,
                    UserName = user
                });
                await logger.InfoAsync($"Client Connected: {user} @ {Context.ConnectionId}");
                await Clients.Others.Connected(user);
            }
            await base.OnConnectedAsync();
        }
        public async override Task OnDisconnectedAsync(Exception exception)
        {
            var client = ClientHandler.Find(Context.ConnectionId);
            if (client != null)
            {
                ClientHandler.RemoveClient(Context.ConnectionId);
                await logger.InfoAsync($"Client Disconnected: {client.UserName} @ {client.ConnectionId}");
                await Clients.Others.Disconnected(client.UserName);
            }
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
            if (client == null)
                return null;
            var gameOpponents = gameService.GetActiveGameOpponents(client.UserName);

            var onlineContacts = ClientHandler.Clients
                .Where(c => gameOpponents.Contains(c.UserName))
                .Select(c => c.UserName)
                .Distinct();
            return Task.FromResult(onlineContacts);
        }
           
        public Task<string> GetGameMatch()
        {
            var player = ClientHandler.Find(Context.ConnectionId);
            if (player == null)
                return null;

            return gameService.GetGameMatch(player.UserName);
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
                
        public async Task BroadCastMessage(string sender, string message)
        {            
            await Clients.All.BroadCastMessage(sender, message);
        }
        public async Task SendChallengePlayer(string userName, IGameChallenge challenge)
        {
            var connection = ClientHandler.FindByUserName(userName);
            if (connection != null && connection.ConnectionId != null)
                await Clients.Client(connection.ConnectionId).NewChallenge(challenge);                
        }
        public async Task SendChallengeCanceled(string userName, string challengeId)
        {
            var connection = ClientHandler.FindByUserName(userName);
            if (connection != null && connection.ConnectionId != null)
                await Clients.Client(connection.ConnectionId).ChallengeCanceled(challengeId);
        }
        public async Task SendChallengeAccepted(string userName, string challengeId, bool accept, string gameId)
        {
            var connection = ClientHandler.FindByUserName(userName);
            if (connection != null && connection.ConnectionId != null)
                await Clients.Client(connection.ConnectionId).ChallengeAccepted(challengeId, accept, gameId);
        }
       
        private async Task SendPlayOk(string connectionId, string gameId, string username)
        {
            if (!string.IsNullOrEmpty(connectionId))
                await Clients.Client(connectionId).PlayOk(gameId, username);
        }
        private async Task SendGameOver(string connectionId, string gameId, GameResult result)
        {
            if (!string.IsNullOrEmpty(connectionId))
                await Clients.Client(connectionId).GameOver(gameId, result);
        }
        
        
    }
}
