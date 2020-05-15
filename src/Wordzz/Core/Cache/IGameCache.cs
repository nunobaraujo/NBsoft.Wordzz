using NBsoft.Wordzz.Contracts;
using NBsoft.Wordzz.Entities;

namespace NBsoft.Wordzz.Core.Cache
{
    public interface IGameCache
    {
        IGame[] ActiveGames { get; }
        void AddGame(IGame game);
        bool RemoveGame(string gameId);

        PendingGame[] PendingGames { get; }
        void AddPending(PendingGame game);
        PendingGame RemovePending(string userName);

    }
}
