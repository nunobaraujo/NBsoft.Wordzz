using System;

namespace NBsoft.Wordzz.Contracts.Entities
{
    [Serializable]
    public class PlayMove : IPlayMove
    {
        public string Player { get; set; }
        public IPlayLetter[] Letters { get; set; }
        public DateTime PlayStart { get; set; }
        public DateTime? PlayFinish { get; set; }
        public IPlayWord[] Words { get; set; }
        public int Score { get; set; }
    }
}
