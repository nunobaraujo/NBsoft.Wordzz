using System.Collections.Generic;

namespace NBsoft.Wordzz.Contracts
{
    public interface IPlayWord
    {        
        IEnumerable<IPlayLetter> Letters { get; }
        int Score { get; }
        int RawScore { get; }
        string Description { get; }
    }
}
