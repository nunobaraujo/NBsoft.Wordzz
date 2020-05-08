using NBsoft.Wordzz.Core.Models;
using System;

namespace NBsoft.Wordzz.Entities
{
    public class GameQueue : IGameQueue
    {
        public string Id { get; set; }
        public DateTime QueueDate { get; set; }
        public string Language { get; set; }
        public string Player1 { get; set; }
        public string Player2 { get; set; }
        public int BoardId { get; set; }
    }
}
