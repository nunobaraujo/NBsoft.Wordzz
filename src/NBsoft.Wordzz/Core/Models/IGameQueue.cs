using System;

namespace NBsoft.Wordzz.Core.Models
{
    public interface IGameQueue
    {
        string Id { get; }
        DateTime QueueDate { get; }
        string Language { get; }
        string Player1 { get; }
        string Player2 { get; }
        int Size { get; }
    }
}
