using System.Collections.Generic;
using System.Linq;

namespace NBsoft.Wordzz.Contracts.Entities
{
    public class GamePlayer : IGamePlayer
    {
        public string Player { get; set; }
        public IEnumerable<IPlayTurn> History { get; set; }
        public IEnumerable<ILetter> Rack { get; set; }        
        public int Score => History?.Sum(x => x.Word.Score) ?? 0;
    }
}
