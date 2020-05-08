using System;

namespace NBsoft.Wordzz.Contracts.Entities
{
    [Serializable]
    public class Board : IBoard
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int BoardRows { get; set; }
        public int BoardColumns { get; set; }
        public BoardTile[] Tiles { get; set; }

        IBoardTile[] IBoard.Tiles => Tiles;
    }
}
