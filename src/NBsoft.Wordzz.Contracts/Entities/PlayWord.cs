using System;

namespace NBsoft.Wordzz.Contracts.Entities
{
    [Serializable]
    public class PlayWord : IPlayWord
    {
        public IPlayLetter[] Letters { get; set; }
        public int Score { get; set; }
    }
}
