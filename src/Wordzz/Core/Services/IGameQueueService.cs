using NBsoft.Wordzz.Contracts;
using NBsoft.Wordzz.Core.Models;
using NBsoft.Wordzz.Entities;
using System.Collections.Generic;

namespace NBsoft.Wordzz.Core.Services
{
    public interface IGameQueueService
    {
        IEnumerable<IGameQueue> AllQueues();

        IGameQueue QueueGame(string language, int boardId, string userName);
        IGameQueue QueueChallenge(string language, int boardId, string challengerName, string challengedNamed);

        bool RemoveQueue(string queueId);
        IGameQueue GetQueue(string queueId);
        
        IEnumerable<IGameQueue> GetQueues(string userName);
        IEnumerable<IGameQueue> GetSentChallenges(string userName);
        IEnumerable<IGameQueue> GetReceivedChallenges(string userName);

        GameMatch DequeueMatch(string userName);
    }
}
