using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NBsoft.Wordzz.Hubs
{
    public class ChatHub : Hub
    {
        public override Task OnConnectedAsync()
        {   
            var user = Context.User?.Identities.FirstOrDefault()?.Name;
            UserHandler.ConnectedIds.Add(Context.ConnectionId, user);
            Console.WriteLine($"{DateTime.UtcNow:HH:mm:ss.fff} - Client Connected: {user}[{Context.ConnectionId}]");
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            var user = UserHandler.ConnectedIds[Context.ConnectionId];
            Console.WriteLine($"{DateTime.UtcNow:HH:mm:ss.fff} - Client Disconnected: {user}[{Context.ConnectionId}]");
            UserHandler.ConnectedIds.Remove(Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }

        public async Task SendToAll(string name, string message)
        {
            await Clients.All.SendAsync("sendToAll", name, message);
        }
    }
}
