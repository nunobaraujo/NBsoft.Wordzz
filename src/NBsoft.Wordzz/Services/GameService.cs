using NBsoft.Wordzz.Contracts;
using NBsoft.Wordzz.Contracts.Entities;
using NBsoft.Wordzz.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NBsoft.Wordzz.Services
{
    internal class GameService : IGameService
    {
        public IBoard GenerateBoard()
        {
            var board = new Board();
            board.Words = new List<IBoardWord>();

            var tiles = new List<BoardTile>();
            for (short i = 1; i <= 15; i++)
            {
                for (short j = 1; j <= 15; j++)
                {
                    tiles.Add(new BoardTile { X = i , Y = j , Bonus = BonusType.None });
                }
            }
            board.Tiles = tiles.Select(x => (IBoardTile)x).ToArray();

            board.ApplyBonusTiles();

            return board;
        }
    }
}
