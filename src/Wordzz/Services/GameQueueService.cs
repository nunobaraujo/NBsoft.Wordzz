using NBsoft.Logs;
using NBsoft.Logs.Interfaces;
using NBsoft.Wordzz.Contracts.Entities;
using NBsoft.Wordzz.Core;
using NBsoft.Wordzz.Core.Models;
using NBsoft.Wordzz.Core.Services;
using NBsoft.Wordzz.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace NBsoft.Wordzz.Services
{
    internal class GameQueueService : IGameQueueService
    {       

        private static SemaphoreSlim semaphore = new SemaphoreSlim(1);

        private readonly List<IGameQueue> gameQueue;
        private readonly ILogger logger;
        private readonly List<GameMatch> gamesMatched;

        public GameQueueService(ILogger logger)
        {
            this.logger = logger;
            gameQueue = new List<IGameQueue>();
            gamesMatched = new List<GameMatch>();
        }

        public IGameQueue QueueGame(string language, int boardId, string userName)
        {
            return QueueChallenge(language, boardId, userName, null);
        }
        public IGameQueue QueueChallenge(string language, int boardId, string challengerName, string challengedNamed)
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
                Player1 = challengerName,
                Player2 = challengedNamed,
                BoardId = boardId,
                QueueDate = DateTime.UtcNow
            };

            logger.Info($"Nem game in queue. P1:{challengerName} P2:{challengedNamed} Language:{language} Board:{boardId}");

            ProcessQueue(newQueue);
            return newQueue;
        }

        public bool RemoveQueue(string queueId)
        {
            var q = GetQueue(queueId);
            if (q != null)
                return gameQueue.Remove(q);
            return false;
        }

        public IGameQueue GetQueue(string queueId) => gameQueue.FirstOrDefault(q => q.Id == queueId);

        public IEnumerable<IGameQueue> GetSentChallenges(string userName)
        {
            // Player 1 is the challenger Player 2 was the challenged player
            // for it to be a challenge, player 2 must not be null
            return gameQueue.Where(q => q.Player1 == userName && !string.IsNullOrEmpty(q.Player2));
        }
        public IEnumerable<IGameQueue> GetReceivedChallenges(string userName)
        {
            // Player 2 was the challenged player, player 1 is the challenger
            return gameQueue.Where(q => q.Player2 == userName);
        }

        public GameMatch DequeueMatch(string userName)
        {
            var match = gamesMatched.FirstOrDefault(m => m.Queue01.Player1 == userName || m.Queue02.Player1 == userName);
            if (match != null)
            {
                gamesMatched.Remove(match);
                return match;
            }
            return null;
        }

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
                    var match = others.FirstOrDefault(o => o.Player1 != first.Player1 && o.Language == first.Language && o.BoardId == first.BoardId);                    

                    if (match == null)
                    {
                        index++;
                    }
                    else
                    {
                        logger.Debug($"Found game match. P1:{first.Player1} P2:{match.Player1} Q1:{first.Id} Q2:{match.Id}");
                        index = 0;
                        // Send match found signal and remove both from queue
                        gamesMatched.Add(new GameMatch { Queue01 = first, Queue02 = match });
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
