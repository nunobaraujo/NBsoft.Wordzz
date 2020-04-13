using System;

namespace NBsoft.Wordzz.Contracts
{
    public interface IPlayTurn
    {
        IBoardWord Word { get; }
        TimeSpan PlayTime { get; }
    }
}
