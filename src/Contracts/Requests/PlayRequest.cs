using NBsoft.Wordzz.Contracts.Entities;
using System;

namespace NBsoft.Wordzz.Contracts.Requests
{
    [Serializable]
    public class PlayRequest
    {
        public string GameId { get; set; }
        public string UserName { get; set; }
        public PlayLetter[] Letters { get; set; }
    }
}
