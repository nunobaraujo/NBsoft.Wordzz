using System;

namespace NBsoft.Wordzz.Contracts.Entities
{
    [Serializable]
    public class BoardTile : IBoardTile
    {
        public int Id { get; set; }
        public int BoardId { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public BonusType Bonus { get; set; }

        
    }
}
