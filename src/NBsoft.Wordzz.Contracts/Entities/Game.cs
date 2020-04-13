using System;
using System.Collections.Generic;

namespace NBsoft.Wordzz.Contracts.Entities
{
    public class Game : IGame
    {
        public IBoard Board { get; set; }
        public IGamePlayer Player01 { get; set; }
        public IGamePlayer Player02 { get; set; }
        public GameStatus Status { get; set; }
        public string CurrentPlayer { get; set; }
        public DateTime CurrentStart { get; set; }
        public DateTime? CurrentPauseStart { get; set; }
        public IEnumerable<ILetter> LetterBag { get; set; }

        public IGamePlayer GetPlayer(string userName)
        {
            if (Player01.Player == userName)
                return Player01;
            else if (Player02.Player == userName)
                return Player02;
            else return null;
        }
    }
}
