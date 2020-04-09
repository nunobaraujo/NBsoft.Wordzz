using NBsoft.Wordzz.Contracts.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NBsoft.Wordzz.Extensions
{
    public static class BoardExtensions
    {
        public static BoardTile Tile(this Board src, int x, int y)
        {
            return src.Tiles.SingleOrDefault(t => t.X == x && t.Y == y) as BoardTile;
        }
    }
}
