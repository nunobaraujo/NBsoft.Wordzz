using System;
using System.Collections.Generic;

namespace NBsoft.Wordzz.Contracts.Entities
{
    [Serializable]
    public class Board : IBoard
    {
        public int Rows { get; set; }
        public int Columns { get; set; }
        public IBoardTile[] Tiles { get; set; }
        public IList<IBoardWord> Words { get; set; }
    }
}
