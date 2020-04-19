using System.Collections.Generic;

namespace NBsoft.Wordzz.Contracts
{
    public interface IBoard
    {
        int Rows { get; }
        int Columns { get; }
        IBoardTile[] Tiles { get; }
        IList<IWordPlay> Words { get; }
    }
}
