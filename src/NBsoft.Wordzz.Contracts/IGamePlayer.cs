using System.Collections.Generic;

namespace NBsoft.Wordzz.Contracts
{
    public interface IGamePlayer
    {
        string UserName { get; }
        string FirstName { get; }
        string LastName { get; }
        IEnumerable<IPlayTurn> History { get; }
        IEnumerable<ILetter> Rack { get; }        
        int Score { get; }
    }
}
