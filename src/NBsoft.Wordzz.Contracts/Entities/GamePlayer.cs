using System;
using System.Collections.Generic;

namespace NBsoft.Wordzz.Contracts.Entities
{
    [Serializable]
    public class GamePlayer : IGamePlayer
    {
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }        
        public IEnumerable<ILetter> Rack { get; set; }
        public int Score { get; set; }
    }
}
