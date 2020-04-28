using Microsoft.AspNetCore.SignalR;
using NBsoft.Wordzz.Contracts;
using NBsoft.Wordzz.Contracts.Entities;
using NBsoft.Wordzz.Contracts.Requests;
using NBsoft.Wordzz.Contracts.Results;
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
        private readonly ILexiconService lexiconService;

        public GameHub(IGameService gameService, ILexiconService lexiconService)
        {
            this.gameService = gameService;
            this.lexiconService = lexiconService;
        }
                
        public async override Task OnConnectedAsync()
        {   
            var user = Context.User?.Identities.FirstOrDefault()?.Name;
            ClientHandler.AddClient(new WordzzClient {
                ConnectionDate = DateTime.UtcNow,
                ConnectionId = Context.ConnectionId,
                UserName = user
            });

            Console.WriteLine($"{DateTime.UtcNow:HH:mm:ss.fff} - Client Connected: {user}[{Context.ConnectionId}]");
            await Clients.Others.SendAsync("connected", user);
            await base.OnConnectedAsync();
        }

        public async override Task OnDisconnectedAsync(Exception exception)
        {
            var user = ClientHandler.Find(Context.ConnectionId);
            Console.WriteLine($"{DateTime.UtcNow:HH:mm:ss.fff} - Client Disconnected: {user?.UserName}[{Context?.ConnectionId}]");
            ClientHandler.RemoveClient(Context.ConnectionId);
            await Clients.Others.SendAsync("disconnected", user.UserName);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendToAll(string name, string message)
        {
            await Clients.All.SendAsync("sendToAll", name, message);
        }
                
        public async Task<IEnumerable<string>> GetOnlineContacts() 
        {
            var client = ClientHandler.Find(Context.ConnectionId);
            var allContacts = await gameService.GetContacts(client.UserName);
            var onlineContacts = ClientHandler.Clients
                .Where(c => allContacts.Contains(c.UserName))
                .Select(c => c.UserName)
                .Distinct();
            return onlineContacts;
        }

        public Task<IEnumerable<IGame>> GetActiveGames()
        {
            var client = ClientHandler.Find(Context.ConnectionId);
            var games = gameService.GetActiveGames(client.UserName);
            return Task.FromResult(games);
        }
       

        public async Task<string> ChallengeGame(string language , string challengedPlayer, int size = 15)
        {
            if (string.IsNullOrEmpty(language))
                language = "en-us";
            
            var challenger = ClientHandler.Find(Context.ConnectionId);
            var opposer = ClientHandler.FindByUserName(challengedPlayer);

            if (opposer == null)
            {
                return null;
            }
            var challengeId = await gameService.ChallengeGame(language, challenger.UserName, opposer.UserName, size);
            string username = challenger.UserName;

            await SendChallengePlayer(opposer.ConnectionId, challengeId, challenger.UserName, language, size);

            return challengeId;
        }
        public async Task<string> ChallengeAccept(string queueId, bool accept)
        {
            if (string.IsNullOrEmpty(queueId))
                return null;

            var challengedPlayer = ClientHandler.Find(Context.ConnectionId);
            var queue = gameService.GetQueuedGame(queueId);
            var game = await gameService.AcceptChallenge(challengedPlayer.UserName, queueId, accept);
            if (queue == null || game == null){
                return null;
            }
            var challengerConnection = ClientHandler.FindByUserName(queue.Player1);            
            await SendChallengeAccepted(challengerConnection.ConnectionId, queue.Id, accept, game?.Id);
            return game?.Id;
        }

        public async Task<PlayResult> Play(PlayRequest playRequest) 
        {   
            var result = await gameService.Play(playRequest.GameId, playRequest.UserName, playRequest.Letters);
            if (result.MoveResult == "OK")
            {
                var game = gameService.GetActiveGames(playRequest.UserName).Single(g => g.Id == playRequest.GameId);
                var opponent = playRequest.UserName == game.Player01.UserName 
                    ? game.Player02.UserName 
                    : game.Player01.UserName;

                var opponentConnection = ClientHandler.FindByUserName(opponent);
                await SendPlayOk(opponentConnection.ConnectionId, playRequest.GameId, playRequest.UserName);
            }
            return result;
        }


        private async Task SendPlayOk(string connectionId, string gameId, string username)
        {
            await Clients.Client(connectionId).SendAsync("playOk", gameId, username);
        }
        private async Task SendChallengePlayer(string connectionId, string id, string username, string language, int size )
        {            
            await Clients.Client(connectionId).SendAsync("newChallenge", id, username, language, size);
        }
        private async Task SendChallengeAccepted(string connectionId, string challengeId, bool accept, string gameId)
        {
            Console.WriteLine("challengeAccepted:", challengeId, accept);
            await Clients.Client(connectionId).SendAsync("challengeAccepted", challengeId, accept, gameId);
        }
    }
}
