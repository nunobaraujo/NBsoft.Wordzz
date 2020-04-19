using System.Collections.Generic;
using System.Linq;

namespace NBsoft.Wordzz.Hubs
{
    public static class ClientHandler
    {
        private static List<WordzzClient> clients = new List<WordzzClient>();
        public static IEnumerable<WordzzClient> Clients => clients.ToArray();

        public static void AddClient(WordzzClient client)
        {
            clients.Add(client);
        }
        public static void RemoveClient(string connectionId)
        {
            var cli = clients.FirstOrDefault(c => c.ConnectionId == connectionId);
            if (cli != null)
                clients.Remove(cli);
        }
        public static WordzzClient Find(string connectionId)
        {
            return clients.FirstOrDefault(x => x.ConnectionId == connectionId);
        }
        public static WordzzClient FindByUserName(string userName)
        {
            var connections = clients.Where(c => c.UserName == userName);
            if (connections == null)
                return null;
            if (connections.Count() > 1)
                return connections
                    .OrderByDescending(c => c.ConnectionDate)
                    .First();

            return connections.First();

        }
        
    }
}
