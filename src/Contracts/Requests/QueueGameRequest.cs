using System;

namespace Wordzz.Contracts.Requests
{
    [Serializable]
    public class QueueGameRequest
    {
        public string Language { get; set; }
        public int BoardId { get; set; }
    }
}
