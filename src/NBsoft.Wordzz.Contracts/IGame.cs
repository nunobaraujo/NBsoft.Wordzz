using System;
using System.Collections.Generic;

namespace NBsoft.Wordzz.Contracts
{
    public interface IGame
    {
        string Id { get; }
        string Language { get; }
        DateTime CreationDate { get; }
        IBoard Board { get; }

        string CurrentPlayer { get; }
        IGamePlayer Player01 { get; }
        IGamePlayer Player02 { get; }        

        GameStatus Status { get; }
        ILetterBag LetterBag { get; set; }        
        DateTime CurrentStart { get; }
        DateTime? CurrentPauseStart { get; }

        IEnumerable<IPlayMove> PlayMoves { get; }
        IEnumerable<char> AvailableLetters { get; }

        IGamePlayer GetPlayer(string userName);
    }
}
