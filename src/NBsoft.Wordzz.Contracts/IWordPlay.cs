using System;

namespace NBsoft.Wordzz.Contracts
{
    public interface IWordPlay
    {
        DateTime PlayDate { get; }
        IBoardLetter[] Letters { get; }
        IBoardTile[] Tiles { get; }
        string Owner { get; }
        int Score { get; }
    }
}
