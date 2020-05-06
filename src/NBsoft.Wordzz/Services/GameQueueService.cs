using NBsoft.Logs;
using NBsoft.Logs.Interfaces;
using NBsoft.Wordzz.Core;
using NBsoft.Wordzz.Core.Models;
using NBsoft.Wordzz.Core.Services;
using NBsoft.Wordzz.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NBsoft.Wordzz.Services
{
    internal class GameQueueService : IGameQueueService
    {
        public event MatchFoundEventDelegate OnMatchFound;

        private static SemaphoreSlim semaphore = new SemaphoreSlim(1);

        private readonly List<IGameQueue> gameQueue;
        private readonly ILogger logger;

        public GameQueueService(ILogger logger)
        {
            this.logger = logger;
            gameQueue = new List<IGameQueue>();
        }

        public IGameQueue QueueGame(string language, string player1UserName, string player2UserName, int boardId)
        {
            /* 
             * Does the queue has player2 ? 
             * If it does game hub will send a game request to player2
             * If player 2 accepts the challenge gamehub will receive an OK with the Queue ID and the that will start
             * Otherwise the queue will removed and the game will be canceled
             * 
             * If the queue doesn't have a player2 search the queue for an awaiting game with the same characteristics
             * if a match is found then start the game
             * if not this stays in queue until a new match is found
            */

            var newQueue = new GameQueue
            {
                Id = Guid.NewGuid().ToString(),
                Language = language,
                Player1 = player1UserName,
                Player2 = player2UserName,
                BoardId = boardId,
                QueueDate = DateTime.UtcNow
            };
            
            logger.Info($"Nem game in queue. P1:{player1UserName} P2:{player2UserName} Language:{language} Board:{boardId}");

            ProcessQueue(newQueue);
            return newQueue;
        }

        public bool RemoveQueue(string queueId)
        {
            var q = GetQueuedGame(queueId);
            if (q != null)
                return gameQueue.Remove(q);
            return false;
        }

        public IGameQueue GetQueuedGame(string queueId) => gameQueue.FirstOrDefault(q => q.Id == queueId);

        private void ProcessQueue(IGameQueue newQueue)
        {
            // block all other threads calling this method
            semaphore.Wait(10 * 1000);
            
            // add new queue
            gameQueue.Add(newQueue);

            // Find Matches
            int index = 0;
            try
            {
                do
                {
                    var waiting = gameQueue
                        .Where(q => q.Player2 == null)
                        .OrderBy(q => q.QueueDate)
                        .ToArray();

                    if (waiting.Length < 1 || index > waiting.Length - 1)
                        break;

                    var first = waiting[index];
                    var others = waiting.Where(q => q.Id != first.Id).ToArray();
                    var match = others.FirstOrDefault(o => o.Language == first.Language && o.BoardId == first.BoardId);

                    if (match == null)
                    {
                        index++;
                    }
                    else
                    {
                        index = 0;
                        // Send match found signal and remove both from queue
                        OnMatchFound?.Invoke(this, new MatchFoundEventArgs { Queue01 = first, Queue02 = match });
                        RemoveQueue(first.Id);
                        RemoveQueue(match.Id);

                    }
                } while (true);
            }
            catch(Exception ex)
            {
                logger.Error("Error processing queue.", ex);
                throw;
            }
            finally
            {
                semaphore.Release();
            }
        }
    }
}
