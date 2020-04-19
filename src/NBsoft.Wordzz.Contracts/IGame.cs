using System;

namespace NBsoft.Wordzz.Contracts
{
    public interface IGame
    {
        string Id { get; }
        string Language { get; }
        IBoard Board { get; }
        IGamePlayer Player01 { get; }
        IGamePlayer Player02 { get; }        
        GameStatus Status { get; }
        ILetterBag LetterBag { get; set; }
        string CurrentPlayer { get; }
        DateTime CurrentStart { get; }
        DateTime? CurrentPauseStart { get; }

        IGamePlayer GetPlayer(string userName);
    }
}
