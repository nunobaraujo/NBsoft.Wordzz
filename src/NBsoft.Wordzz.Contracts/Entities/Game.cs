using System;
using System.Collections.Generic;

namespace NBsoft.Wordzz.Contracts.Entities
{
    public class Game : IGame
    {
        public string Id { get; set; }
        public string Language { get; set; }
        public DateTime CreationDate { get; set; }
        public IBoard Board { get; set; }
        public IGamePlayer Player01 { get; set; }
        public IGamePlayer Player02 { get; set; }
        public GameStatus Status { get; set; }
        public string CurrentPlayer { get; set; }
        public DateTime CurrentStart { get; set; }
        public DateTime? CurrentPauseStart { get; set; }
        public ILetterBag LetterBag { get; set; }
        public IEnumerable<IPlayMove> PlayMoves { get; set; }
                
        public IGamePlayer GetPlayer(string userName)
        {
            if (Player01.UserName == userName)
                return Player01;
            else if (Player02.UserName == userName)
                return Player02;
            else return null;
        }
    }
}
