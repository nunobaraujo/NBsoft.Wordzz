using System;

namespace NBsoft.Wordzz.Contracts.Entities
{
    [Serializable]
    public class BoardTile : IBoardTile
    {
        public short X { get; set; }
        public short Y { get; set; }
        public BonusType Bonus { get; set; }
    }
}
