using NBsoft.Logs;
using NBsoft.Logs.Interfaces;
using NBsoft.Wordzz.Contracts;
using NBsoft.Wordzz.Contracts.Entities;
using NBsoft.Wordzz.Core.Models;
using NBsoft.Wordzz.Core.Repositories;
using NBsoft.Wordzz.Core.Services;
using NBsoft.Wordzz.Entities;
using NBsoft.Wordzz.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NBsoft.Wordzz.Services
{
    internal class GameService : IGameService
    {
        private readonly IUserRepository userRepository;
        private readonly IWordRepository wordRepository;
        private readonly List<IGame> activeGames;
        private readonly List<IGameQueue> gameQueue;
        private readonly ILogger logger;

        public GameService(IUserRepository userRepository, IWordRepository wordRepository, ILogger logger)
        {
            this.userRepository = userRepository;
            this.wordRepository = wordRepository;
            this.logger = logger;

            activeGames = new List<IGame>();
            gameQueue = new List<IGameQueue>();
        }

        public Task<IEnumerable<string>> GetContacts(string userId) => userRepository.GetContacts(userId);

        public IEnumerable<IGame> GetActiveGames(string userName) => activeGames.Where(x => x.Player01.UserName == userName || x.Player02.UserName == userName);

        public IGameQueue GetQueuedGame(string queueId) => gameQueue.FirstOrDefault(q => q.Id == queueId);        

        public string ChallengeGame(string language, string player1, string player2, int size)
        {
            var queue = QueueGame(language, player1, player2, size);
            return queue.Id;
        }
        public async Task<IGame> AcceptChallenge(string challengedPlayer, string queueId, bool accept)
        {
            if (!accept) // Challenge refused
            {
                // Remove Queue and return null
                RemoveQueue(queueId);
                //TODO: Update user stats, refused games
                return null;
            }

            return await StartGame(queueId);
        }
       


        private IGameQueue QueueGame(string language, string player1UserName, string player2UserName, int size)
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
                Size = size,
                QueueDate = DateTime.UtcNow
            };
            gameQueue.Add(newQueue);
            logger.Info($"Nem game in queue. P1:{player1UserName} P2:{player2UserName} Language:{language} Size:{size}");
            return newQueue;
        }
        private void RemoveQueue(string queueId)
        {
            var q = GetQueuedGame(queueId);
            if (q != null)
                gameQueue.Remove(q);
        }

        private async Task<IGame> NewGame(string language, string player1UserName, string player2UserName, int size)
        {
            var user1 = await userRepository.Get(player1UserName);
            if (user1 == null)
                throw new ApplicationException($"Invalid player 1 [{player1UserName}]");

            var user2 = await userRepository.Get(player2UserName);
            if (user2 == null)
                throw new ApplicationException($"Invalid player 2 [{player2UserName}]");

            return await CreateGame(language, user1, user2, size);
        }
        private async Task<IGame> CreateSoloGame(string language, string player1UserName, int aiLevel, int size)
        {
            var user1 = await userRepository.Get(player1UserName);
            if (user1 == null)
                throw new ApplicationException($"Invalid player 1 [{player1UserName}]");

            return await CreateGame(language, user1, GetAI(aiLevel), size);

        }
        private async Task<IGame> CreateGame(string language, IUser player01, IUser player02, int size)
        {
            var lexicon = await wordRepository.GetDictionary(language);
            if (lexicon == null)
                throw new ApplicationException($"Invalid language [{language}]");

            var p1Details = await userRepository.GetDetails(player01.UserName);
            var p2Details = await userRepository.GetDetails(player02.UserName);

            var letterbag = new LetterBag(language);
            var game = new Game
            {
                Id = Guid.NewGuid().ToString(),
                Language = language,
                CreationDate = DateTime.UtcNow,
                Board = GenerateBoard(size),
                LetterBag = letterbag,
                PlayMoves = new List<IPlayMove>(),
                Player01 = new GamePlayer
                {
                    UserName = player01.UserName,
                    FirstName = p1Details.FirstName,
                    LastName = p1Details.LastName,                    
                    Rack = letterbag.TakeLetters(7),
                    Score = 0
                                   },
                Player02 = new GamePlayer
                {
                    UserName = player02.UserName,
                    FirstName = p2Details.FirstName,
                    LastName = p2Details.LastName,                    
                    Rack = letterbag.TakeLetters(7),
                    Score = 0
                },
                CurrentPlayer = player01.UserName,
                Status = GameStatus.Ongoing,
                CurrentStart = DateTime.UtcNow
            };            
            return game;
        }

        private async Task<IGame> StartGame(string queueId)
        {
            var q = gameQueue.SingleOrDefault(x => x.Id == queueId);
            if (q == null)
                return null;

            var game = await NewGame(q.Language, q.Player1, q.Player2, q.Size);
            
            activeGames.Add(game);
            RemoveQueue(q.Id);

            return game;
        }
        private void FinishGame(string gameId)
        {
            // Todo end game logic
            var game = activeGames.SingleOrDefault(x => x.Id == gameId);
            if (game != null)
                activeGames.Remove(game);
        }

        private IBoard GenerateBoard(int size)
        {
            var board = new Board();
            board.Rows = size;
            board.Columns = size;
            
            var tiles = new List<BoardTile>();
            for (int y = 1; y <= board.Columns; y++)
            {
                for (int x = 1; x <= board.Rows; x++)
                {                
                    tiles.Add(new BoardTile { X = x, Y = y, Bonus = BonusType.Regular });
                }
            }
            board.Tiles = tiles.Select(x => (IBoardTile)x).ToArray();

            var newBoard = board.ApplyBonusTiles();

            return newBoard; ;
        }

        private IUser GetAI(int aiLevel)
        {
            throw new NotImplementedException();
        }

      
    }
}
