using System.Collections.Generic;

namespace NBsoft.Wordzz.Contracts
{
    public interface IGamePlayer
    {
        string Player { get; }
        IEnumerable<IPlayTurn> History { get; }
        IEnumerable<ILetter> Rack { get; }        
        int Score { get; }
    }
}
