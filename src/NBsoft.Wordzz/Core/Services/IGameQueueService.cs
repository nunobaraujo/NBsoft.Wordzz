using NBsoft.Wordzz.Core.Models;

namespace NBsoft.Wordzz.Core.Services
{
    public interface IGameQueueService
    {
        IGameQueue QueueGame(string language, string player1UserName, string player2UserName, int boardId);
        bool RemoveQueue(string queueId);
        IGameQueue GetQueuedGame(string queueId);
        event MatchFoundEventDelegate OnMatchFound;
    }
}
