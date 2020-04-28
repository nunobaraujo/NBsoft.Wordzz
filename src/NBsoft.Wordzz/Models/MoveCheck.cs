using NBsoft.Wordzz.Contracts;
using System.Collections.Generic;

namespace NBsoft.Wordzz.Models
{
    public class MoveCheck
    {
        public string Result { get; set; }
        public IEnumerable<IPlayWord> Words { get; set; }
    }
}
