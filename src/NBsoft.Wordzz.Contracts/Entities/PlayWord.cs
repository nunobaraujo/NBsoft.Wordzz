using System;

namespace NBsoft.Wordzz.Contracts.Entities
{
    [Serializable]
    public class PlayWord : IPlayWord
    {
        public IPlayLetter[] Letters { get; set; }
        public int Score { get; set; }
        public int RawScore { get; set; }
        public string Description { get; set; }
    }
}
