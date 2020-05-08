using NBsoft.Wordzz.Core.Models;

namespace NBsoft.Wordzz.Entities
{
    public class GameMatch
    {
        public IGameQueue Queue01 { get; set; }
        public IGameQueue Queue02 { get; set; }
    }
}
