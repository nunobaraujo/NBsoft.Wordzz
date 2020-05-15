using NBsoft.Logs;
using NBsoft.Logs.Interfaces;
using NBsoft.Wordzz.Contracts;
using NBsoft.Wordzz.Contracts.Entities;
using NBsoft.Wordzz.Core.Cache;
using NBsoft.Wordzz.Core.Repositories;
using NBsoft.Wordzz.Entities;
using NBsoft.Wordzz.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NBsoft.Wordzz.Cache
{
    public class GameCache: IGameCache
    {
        private readonly ILogger logger;
        private readonly IGameRepository gameRepository;
        private readonly IUserRepository userRepository;
        private readonly IBoardRepository boardRepository;

        private readonly List<IGame> activeGames;
        private readonly List<PendingGame> pendingGames;

        public GameCache(ILogger logger, IGameRepository gameRepository, IUserRepository userRepository, IBoardRepository boardRepository)
        {
            this.logger = logger;
            this.gameRepository = gameRepository;
            this.userRepository = userRepository;
            this.boardRepository = boardRepository;

            activeGames = new List<IGame>();
            pendingGames = new List<PendingGame>();

            Task.Run(async () => await LoadGames()).Wait();
        }

        public IGame[] ActiveGames => activeGames.ToArray();
        public PendingGame[] PendingGames => pendingGames.ToArray();

        public void AddGame(IGame game)
        {
            if (game == null)
                throw new ArgumentNullException(nameof(game));
            activeGames.Add(game);
            logger.Info($"Added active game.", context: game.Id);
        }
        public bool RemoveGame(string gameId)
        {
            var game = activeGames.SingleOrDefault(g => g.Id == gameId);
            if (game != null)
            {
                logger.Info($"Removed active game.", context: game.Id);
                return activeGames.Remove(game);
            }
            return false;
        }

        public void AddPending(PendingGame game)
        {
            if (game == null)
                throw new ArgumentNullException(nameof(game));            
            pendingGames.Add(game);
            logger.Info($"Added pending game.", context: $"{game.GameId}:{game.UserName}");
        }

        public PendingGame RemovePending(string userName)
        {
            var pending = pendingGames.FirstOrDefault(p => p.UserName == userName);
            if (pending != null)
            {
                logger.Info($"Removed pending game.", context: $"{pending.GameId}:{pending.UserName}");
                pendingGames.Remove(pending);
                return pending;
            }
            return null;
        }

        private async Task LoadGames()
        {
            var activeG = await gameRepository.GetActive();
            foreach (var g in activeG)
            {
                activeGames.Add(await LoadGame(g));
            }
            await logger.InfoAsync("Active games loaded.");

        }

        private async Task<IGame> LoadGame(GameDataModel src)
        {
            var player01 = await userRepository.Get(src.Player01);
            var player02 = await userRepository.Get(src.Player02);
            var p1Details = await userRepository.GetDetails(player01.UserName);
            var p2Details = await userRepository.GetDetails(player02.UserName);

            var moves = (await gameRepository.GetMoves(src.Id))
                .OrderBy(m => m.PlayStart)
                .ToArray();
            var playMoves = moves.Select(m => new PlayMove
            {
                Player = m.PlayerId,
                PlayStart = m.PlayStart,
                PlayFinish = m.PlayFinish,
                Score = m.Score,
                Letters = m.Letters.FromJson<PlayLetter[]>(),
                Words = m.Words.FromJson<PlayWord[]>()
            });

            return new Game
            {
                Id = src.Id,
                Board = await boardRepository.Get(src.BoardId),
                Language = src.Language,
                CreationDate = src.CreationDate,
                Player01 = new GamePlayer
                {
                    UserName = player01.UserName,
                    FirstName = p1Details.FirstName,
                    LastName = p1Details.LastName,
                    Rack = src.Player01Rack.GetLetters(src.Language),
                    Score = playMoves.Where(m => m.Player == player01.UserName).Sum(m => m.Score)
                },
                Player02 = new GamePlayer
                {
                    UserName = player02.UserName,
                    FirstName = p2Details.FirstName,
                    LastName = p2Details.LastName,
                    Rack = src.Player02Rack.GetLetters(src.Language),
                    Score = playMoves.Where(m => m.Player == player02.UserName).Sum(m => m.Score)
                },
                Status = (GameStatus)src.Status,
                CurrentPlayer = src.CurrentPlayer,
                CurrentStart = src.CurrentStart,
                CurrentPauseStart = src.CurrentPauseStart,
                LetterBag = new LetterBag(src.Language, src.LetterBag.FromJson<Letter[]>()),
                Winner = src.Winner,
                FinishReason = (FinishReason?)src.FinishReason,
                ConsecutivePasses = src.ConsecutivePasses,
                FinishDate = src.FinishDate,
                P1FinalScore = src.P1FinalScore,
                P2FinalScore = src.P2FinalScore,
                AvailableLetters = src.Language.GetLettersOnly(),
                PlayMoves = playMoves
            };

        }

        
    }
}
