using System;
using System.Collections.Generic;

namespace NBsoft.Wordzz.Contracts
{
    public interface IGame
    {
        string Id { get; }
        IBoard Board { get; }
        string Language { get; }
        DateTime CreationDate { get; }
        string CurrentPlayer { get; }
        
        IGamePlayer Player01 { get; }
        IGamePlayer Player02 { get; }
        ILetterBag LetterBag { get; set; }

        GameStatus Status { get; }        
        DateTime CurrentStart { get; }
        DateTime? CurrentPauseStart { get; }
                
        IEnumerable<char> AvailableLetters { get; }

        string Winner { get; }
        FinishReason? FinishReason { get; }
        int ConsecutivePasses { get; }
        DateTime? FinishDate { get; }
        int P1FinalScore { get; }
        int P2FinalScore { get; }

        IEnumerable<IPlayMove> PlayMoves { get; }

        IGamePlayer GetPlayer(string userName);
    }
}
