using System;

namespace NBsoft.Wordzz.Contracts.Entities
{
    [Serializable]
    public class Board : IBoard
    {
        public int Rows { get; set; }
        public int Columns { get; set; }
        public BoardTile[] Tiles { get; set; }

        IBoardTile[] IBoard.Tiles => Tiles;
    }
}
