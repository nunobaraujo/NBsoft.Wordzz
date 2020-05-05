using System;
using System.Collections.Generic;

namespace NBsoft.Wordzz.Contracts.Entities
{
    [Serializable]
    public class PlayWord : IPlayWord
    {
        public IEnumerable<PlayLetter> Letters { get; set; }
        public int Score { get; set; }
        public int RawScore { get; set; }
        public string Description { get; set; }

        IEnumerable<IPlayLetter> IPlayWord.Letters => Letters;
    }
}
