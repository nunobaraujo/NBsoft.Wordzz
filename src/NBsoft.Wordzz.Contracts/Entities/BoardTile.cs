using System;

namespace NBsoft.Wordzz.Contracts.Entities
{
    [Serializable]
    public class BoardTile : IBoardTile
    {
        public int X { get; set; }
        public int Y { get; set; }
        public BonusType Bonus { get; set; }
    }
}
