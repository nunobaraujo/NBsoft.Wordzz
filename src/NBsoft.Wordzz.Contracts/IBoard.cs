using System.Collections.Generic;

namespace NBsoft.Wordzz.Contracts
{
    public interface IBoard
    {
        IBoardTile[] Tiles { get; }
        IList<IBoardWord> Words { get; }
    }
}
