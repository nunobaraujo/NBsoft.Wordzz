using System;

namespace NBsoft.Wordzz.Contracts
{
    public class PlayTurn : IPlayTurn
    {
        public IBoardWord Word { get; set; }
        public TimeSpan PlayTime { get; set; }        
    }
}
