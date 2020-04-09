using System;
using System.Collections.Generic;

namespace NBsoft.Wordzz.Contracts.Entities
{
    [Serializable]
    public class Board : IBoard
    {
        public IBoardTile[] Tiles { get; set; }
        public IList<IBoardWord> Words { get; set; }
    }
}
