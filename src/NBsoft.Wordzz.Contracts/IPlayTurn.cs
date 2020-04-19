using System;

namespace NBsoft.Wordzz.Contracts
{
    public interface IPlayTurn
    {
        IWordPlay Word { get; }
        TimeSpan PlayTime { get; }
    }
}
