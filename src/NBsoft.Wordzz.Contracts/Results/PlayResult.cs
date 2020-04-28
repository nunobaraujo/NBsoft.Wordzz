using NBsoft.Wordzz.Contracts.Entities;
using System;

namespace NBsoft.Wordzz.Contracts.Results
{
    [Serializable]
    public class PlayResult
    {
        public string MoveResult { get; set; }
        public PlayMove PlayMove { get; set; }
    }
}
