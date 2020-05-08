using System;

namespace NBsoft.Wordzz.Contracts
{
    public interface IPlayMove
    {
        string Player { get; }
        IPlayLetter[] Letters { get; }        
        DateTime PlayStart { get; }
        DateTime? PlayFinish { get; }
        IPlayWord[] Words { get; }
        int Score { get; }
    }
}
