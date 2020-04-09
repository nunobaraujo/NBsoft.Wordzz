using NBsoft.Wordzz.Contracts;
using NBsoft.Wordzz.Contracts.Entities;
using NBsoft.Wordzz.Core.Services;
using NBsoft.Wordzz.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NBsoft.Wordzz.Services
{
    internal class GameService : IGameService
    {
        public IBoard GenerateBoard(int rows, int columns)
        {
            var board = new Board();
            board.Rows = rows;
            board.Columns = columns;
            board.Words = new List<IBoardWord>();

            var tiles = new List<BoardTile>();
            for (int x = 1; x <= rows; x++)
            {
                for (int y = 1; y <= 15; y++)
                {
                    tiles.Add(new BoardTile { X = x , Y = y , Bonus = BonusType.Regular });
                }
            }
            board.Tiles = tiles.Select(x => (IBoardTile)x).ToArray();

            board = ApplyBonusTiles(board);

            return board;
        }

        private Board ApplyBonusTiles(Board board)
        {
            if (board.Rows == 15 && board.Columns == 15 )
            {
                // Center
                board.Tile(8, 8).Bonus = BonusType.Center;

                // TripleWord
                board.Tile(1, 1).Bonus = BonusType.TripleWord;
                board.Tile(1, 8).Bonus = BonusType.TripleWord;
                board.Tile(1, 15).Bonus = BonusType.TripleWord;
                board.Tile(8, 1).Bonus = BonusType.TripleWord;
                board.Tile(8, 15).Bonus = BonusType.TripleWord;
                board.Tile(15, 1).Bonus = BonusType.TripleWord;
                board.Tile(15, 8).Bonus = BonusType.TripleWord;
                board.Tile(15, 15).Bonus = BonusType.TripleWord;

                // DoubleWord
                board.Tile(2, 2).Bonus = BonusType.DoubleWord;
                board.Tile(2, 14).Bonus = BonusType.DoubleWord;
                board.Tile(3, 3).Bonus = BonusType.DoubleWord;
                board.Tile(3, 13).Bonus = BonusType.DoubleWord;
                board.Tile(4, 4).Bonus = BonusType.DoubleWord;
                board.Tile(4, 12).Bonus = BonusType.DoubleWord;
                board.Tile(5, 5).Bonus = BonusType.DoubleWord;
                board.Tile(5, 11).Bonus = BonusType.DoubleWord;
                board.Tile(11, 5).Bonus = BonusType.DoubleWord;
                board.Tile(11, 11).Bonus = BonusType.DoubleWord;
                board.Tile(11, 5).Bonus = BonusType.DoubleWord;
                board.Tile(11, 11).Bonus = BonusType.DoubleWord;
                sdf

                // TripleLetter
                board.Tile(2, 2).Bonus = BonusType.DoubleWord;
            }

        }
    }
}
