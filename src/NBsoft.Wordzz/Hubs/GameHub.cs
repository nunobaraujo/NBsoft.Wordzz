using Microsoft.AspNetCore.SignalR;
using NBsoft.Wordzz.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NBsoft.Wordzz.Hubs
{
    public class GameHub : Hub
    {
        private readonly IGameService gameService;

        public GameHub(IGameService gameService)
        {
            this.gameService = gameService;
        }

        public const string Address = "/hubs/game";
        public async override Task OnConnectedAsync()
        {   
            var user = Context.User?.Identities.FirstOrDefault()?.Name;
            ClientHandler.ConnectedIds.Add(Context.ConnectionId, user);
            Console.WriteLine($"{DateTime.UtcNow:HH:mm:ss.fff} - Client Connected: {user}[{Context.ConnectionId}]");
            await Clients.Others.SendAsync("connected", user);
            await base.OnConnectedAsync();
        }

        public async override Task OnDisconnectedAsync(Exception exception)
        {
            var user = ClientHandler.ConnectedIds[Context.ConnectionId];
            Console.WriteLine($"{DateTime.UtcNow:HH:mm:ss.fff} - Client Disconnected: {user}[{Context.ConnectionId}]");
            ClientHandler.ConnectedIds.Remove(Context.ConnectionId);
            await Clients.Others.SendAsync("disconnected", user);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendToAll(string name, string message)
        {
            await Clients.All.SendAsync("sendToAll", name, message);
        }
                
        public async Task<IEnumerable<string>> GetOnlineContacts() 
        {
            var userId = ClientHandler.ConnectedIds[Context.ConnectionId];
            var allContacts = await gameService.GetContacts(userId);
            var onlineContacts = ClientHandler.ConnectedIds
                .Where(c => allContacts.Contains(c.Value))
                .Select(c => c.Value)
                .Distinct();
            return onlineContacts;
        }


        public async Task<string> NewGame(string language , string opposingPlayer, int size = 15)
        {
            if (string.IsNullOrEmpty(language))
                language = "en-us";
            var userId = ClientHandler.ConnectedIds[Context.ConnectionId];
            var game = await gameService.NewGame(language, userId, opposingPlayer, size);            
            return game.Id;
        }
    }
}
