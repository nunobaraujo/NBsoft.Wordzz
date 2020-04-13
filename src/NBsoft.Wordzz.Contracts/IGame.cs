using System;
using System.Collections.Generic;

namespace NBsoft.Wordzz.Contracts
{
    public interface IGame
    {
        string Language { get; }
        IBoard Board { get; }
        IGamePlayer Player01 { get; }
        IGamePlayer Player02 { get; }        
        GameStatus Status { get; }
        IEnumerable<ILetter> LetterBag { get; set; }
        string CurrentPlayer { get; }
        DateTime CurrentStart { get; }
        DateTime? CurrentPauseStart { get; }

        IGamePlayer GetPlayer(string userName);
    }
}
