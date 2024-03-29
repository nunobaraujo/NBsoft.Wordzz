﻿using NBsoft.Logs;
using NBsoft.Logs.Interfaces;
using NBsoft.Wordzz.Contracts;
using NBsoft.Wordzz.Contracts.Entities;
using NBsoft.Wordzz.Contracts.Results;
using NBsoft.Wordzz.Core.Cache;
using NBsoft.Wordzz.Core.Models;
using NBsoft.Wordzz.Core.Repositories;
using NBsoft.Wordzz.Core.Services;
using NBsoft.Wordzz.Entities;
using NBsoft.Wordzz.Extensions;
using NBsoft.Wordzz.GameLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NBsoft.Wordzz.Services
{
    internal class GameService : IGameService
    {        
        private readonly IUserRepository userRepository;
        private readonly IGameRepository gameRepository;
        private readonly IBoardRepository boardRepository;

        private readonly IGameCache gameCache;

        private readonly ILexiconService lexiconService;
        private readonly IStatService statService;
        private readonly IGameQueueService gameQueueService;
        
        private readonly ILogger logger;

        public GameService(ILogger logger,
            IGameCache gameCache,
            IUserRepository userRepository, IGameRepository gameRepository, IBoardRepository boardRepository, 
            ILexiconService lexiconService, IStatService statService, IGameQueueService gameQueueService)
        {
            this.userRepository = userRepository;
            this.gameRepository = gameRepository;
            this.boardRepository = boardRepository;

            this.gameCache = gameCache;

            this.lexiconService = lexiconService;
            this.statService = statService;
            this.gameQueueService = gameQueueService;
            this.logger = logger;
        }

        public async Task<string> GetGameMatch(string userName)
        {            
            var queue = gameQueueService.DequeueMatch(userName);
            if (queue != null)
            {
                var newGame = await StartGame(queue.Queue01, queue.Queue02);
                if (queue.Queue01.Player1 == userName)
                    gameCache.AddPending(new PendingGame { UserName = queue.Queue02.Player1, GameId = newGame.Id });
                else
                    gameCache.AddPending(new PendingGame { UserName = queue.Queue01.Player1, GameId = newGame.Id});
                return newGame.Id;
            }
            else
            {
                var pending = gameCache.RemovePending(userName);
                if (pending != null)
                {
                    return pending.GameId;
                }
            }
            return null;
        }        

        public async Task<IGameChallenge> ChallengeGame(string language, int boardId, string challenger, string challenged)
        {
            if (boardId == 0)
            {
                var b = await GetDefaultBoard();
                boardId = b.Id;
            }

            var lexicon = await lexiconService.GetDictionary(language);
            if (lexicon == null)
                return null;
            var board = await GetBoard(boardId);
            if (board == null)
                return null;

            var q = gameQueueService.QueueChallenge(lexicon.Language, board.Id, challenger, challenged);
            return new GameChallenge { Id = q.Id, Language = q.Language, BoardId = q.BoardId, Origin = q.Player1, Destination = q.Player2 };
        }        
        public async Task<IGame> AcceptChallenge(string queueId, bool accept)
        {
            var q = gameQueueService.GetQueue(queueId);
            if (q == null)
                return null;

            // Remove challenge from Queue
            gameQueueService.RemoveQueue(queueId);


            if (!accept) // Challenge refused
                return null;    //TODO: Update user stats, refused games

            // Start game;
            return await StartGame(q);
        }

        public Task<IEnumerable<string>> GetContacts(string userId) => userRepository.GetContacts(userId);
        public IEnumerable<IGame> GetActiveGames(string userName) => gameCache.ActiveGames.Where(x => x.Player01.UserName == userName || x.Player02.UserName == userName);
        public IEnumerable<IGame> GetAllActiveGames() => gameCache.ActiveGames;
        public IEnumerable<string> GetActiveGameOpponents(string userName) 
        {
            var userGames = GetActiveGames(userName);
            var opponents = userGames.Select(x => x.Player01.UserName)
                .Concat(userGames.Select(x => x.Player02.UserName))
                .Distinct()
                .ToList();
            opponents.Remove(userName);

            return opponents;
        }

        public async Task<PlayResult> Play(string gameId, string username, PlayLetter[] letters)
        {
            var game = gameCache.ActiveGames.SingleOrDefault(g => g.Id == gameId) as Game;
            if (game == null)
            {
                string message = $"Invalid game: {gameId}";
                await logger.WarningAsync(message, context: $"{gameId}:{username}");
                return new PlayResult { MoveResult = message };
            }
            
            //  Play must be done by current player
            var player = game.GetPlayer(username);
            if (player.UserName != game.CurrentPlayer)
            {
                string message = $"Game Play received not from current player. (Game:{gameId} Current Player:{game.CurrentPlayer}) ";
                await logger.WarningAsync(message, context: $"{gameId}:{username}");
                return new PlayResult { MoveResult = message };
            }

            // Played letters must be int player rack
            var rack = player.Rack.Select(r => r.Char).ToList();
            foreach (var letter in letters)
            {
                if (letter.Letter.Letter.IsBlank) 
                {
                    var blankInRack = rack.Contains(' ');
                    if (!blankInRack)
                    {
                        string message = "Player rack doesn't contain blank tiles";
                        await logger.InfoAsync(message, context: $"{gameId}:{username}");
                        return new PlayResult { MoveResult = message };
                    }
                }
                else
                {
                    var letterInRack = rack.Contains(letter.Letter.Letter.Char);
                    if (!letterInRack)
                    {
                        string message = $"Player rack doesn't contain {letter.Letter.Letter.Char}";
                        await logger.InfoAsync(message, context: $"{gameId}:{username}");
                        return new PlayResult { MoveResult = message };
                    }
                }
            }

            // Validate Move
            var validationResult = await game.ValidateMove(letters, lexiconService);
            if (validationResult.Result != "OK")
            {
                string message = $"Invalid move: {validationResult.Result} ";
                //await logger.InfoAsync(message, context: username);
                return new PlayResult { MoveResult = message };
            }

            // Score the words
            var scoreWords = game.ScoreMove(validationResult.Words, letters);
                       

            // Create move entity
            var playFinish = new DateTime(DateTime.UtcNow.Ticks, DateTimeKind.Utc);
            var move = new PlayMove
            {
                Letters = letters,
                Player = player.UserName,
                PlayStart = new DateTime(game.CurrentStart.Ticks, DateTimeKind.Utc),
                PlayFinish = playFinish,
                Words = scoreWords.ToArray(),
                Score = scoreWords.Sum(w => w.Score)
            };

            // Update game time, and current player and reset ConsecutivePasses
            var opponent = game.Player01.UserName == player.UserName ? game.Player02 : game.Player01;
            game.CurrentStart = playFinish;
            game.CurrentPlayer = opponent.UserName;
            game.ConsecutivePasses = 0;

            // Update game moves
            var moves = game.PlayMoves.ToList();
            moves.Add(move);
            game.PlayMoves = moves;

            // TODO: update player rack
            var ePlayer = player as GamePlayer;
            var eRack = player.Rack.ToList();
            var lettersToRemove = letters.Select(l => l.Letter).ToList();
            foreach (var letter in lettersToRemove)
            {
                var rLetter = letter.Letter.IsBlank
                    ? eRack.First(l => l.IsBlank)
                    : eRack.First(l => l.Char == letter.Letter.Char);
                eRack.Remove(rLetter);
            }
            var lettersNeeded = 7 - eRack.Count();
            var newLetters = game.LetterBag.TakeLetters(lettersNeeded);
            ePlayer.Rack = eRack.Concat(newLetters);
            ePlayer.Score = moves.Where(m => m.Player == ePlayer.UserName).Sum(m => m.Score);

            
            
            // Save game state to DB
            await gameRepository.Update(game.ToDataModel());
            await gameRepository.AddMoves(game.Id, new GameMoveDataModel[] { move.ToDataModel() });

            //logger.Info($"Game move letter:[{letters.GetString()}] Words:[{string.Join(",", move.Words.Select(w => w.GetString() + "=" + w.Score))}]",context: player.UserName);
            await logger.InfoAsync($"PLAY! = Duration:[{Math.Round((move.PlayFinish.Value - move.PlayStart).TotalMinutes,2)}] Letters:[{letters.GetString()}] Words:[{string.Join(",", move.Words.Select(w => w.GetString() + "=" + w.Score))}]", context: $"{gameId}:{username}");
            return new PlayResult
            {
                MoveResult = "OK",
                PlayMove = move
            }; 
        }
        public async Task<PlayResult> Pass(string gameId, string username)
        {
            Game game = gameCache.ActiveGames.SingleOrDefault(g => g.Id == gameId) as Game;
            if (game == null)
            {
                string message = $"Invalid game: {gameId}";
                await logger.WarningAsync(message, context: $"{username}:{gameId}");
                return new PlayResult { MoveResult = message };
            }

            //  Play must be done by current player
            var player = game.GetPlayer(username);
            if (player.UserName != game.CurrentPlayer)
            {
                string message = $"Game Play received not from current player. (Game:{gameId} Current Player:{game.CurrentPlayer}) ";
                await logger.WarningAsync(message, context: $"{gameId}:{username}");
                return new PlayResult { MoveResult = message };
            }

            
            // Create move entity
            var playFinish = new DateTime(DateTime.UtcNow.Ticks, DateTimeKind.Utc);
            var move = new PlayMove
            {
                Letters = new IPlayLetter[0],
                Player = player.UserName,
                PlayStart = new DateTime(game.CurrentStart.Ticks, DateTimeKind.Utc),
                PlayFinish = playFinish,
                Words = new IPlayWord[0],
                Score = 0
            };

            // Update game time, and current player
            var opponent = game.Player01.UserName == player.UserName ? game.Player02 : game.Player01;
            game.CurrentStart = playFinish;
            game.CurrentPlayer = opponent.UserName;
            game.ConsecutivePasses++;

            // Update game moves
            var moves = game.PlayMoves.ToList();
            moves.Add(move);
            game.PlayMoves = moves;
            
            await logger.InfoAsync($"PASS! = Duration:[{Math.Round((move.PlayFinish.Value - move.PlayStart).TotalMinutes, 2)}]", context: $"{gameId}:{username}");
                        
            
            await gameRepository.Update(game.ToDataModel());
            await gameRepository.AddMoves(game.Id, new GameMoveDataModel[] { move.ToDataModel() });

            if (game.ConsecutivePasses >= 4)
            {
                return new PlayResult
                {
                    MoveResult = "GameOver",
                    PlayMove = move,
                    GameOverResult = await FinishGame(game.Id, FinishReason.ConsecutivePass)
                };
            }
            else 
            {
                return new PlayResult
                {
                    MoveResult = "OK",
                    PlayMove = move,
                    GameOverResult = null
                };
            }
                            
            
        }
        public async Task<PlayResult> Forfeit(string gameId, string username)
        {
            var game = gameCache.ActiveGames.SingleOrDefault(g => g.Id == gameId) as Game;
            if (game == null)
            {
                string message = $"Invalid game: {gameId}";
                await logger.WarningAsync(message, context: $"{username}:{gameId}");
                return new PlayResult { MoveResult = message };
            }

            //  Play must be done by current player
            var player = game.GetPlayer(username);           

            // Create move entity
            var playFinish = new DateTime(DateTime.UtcNow.Ticks, DateTimeKind.Utc);
            
            // Update game time, and current player
            var opponent = game.Player01.UserName == player.UserName ? game.Player02 : game.Player01;
            game.CurrentStart = playFinish;
            
            await logger.InfoAsync($"FORFEIT!", context: $"{gameId}:{username}");

            return new PlayResult
            {
                MoveResult = "GameOver",
                PlayMove = null,
                GameOverResult = await FinishGame(game.Id, FinishReason.Forfeit, username)
            };
        }

        #region Privates


        private async Task<IGame> NewSoloGame(string language, int boardId, string player1UserName, int aiLevel)
        {
            var user1 = await userRepository.Get(player1UserName);
            if (user1 == null)
                throw new ApplicationException($"Invalid player 1 [{player1UserName}]");

            return await CreateGame(language, boardId, user1, GetAI(aiLevel));

        }
        private async Task<IGame> NewMultiPlayerGame(string language, int boardId, string player1UserName, string player2UserName)
        {
            var user1 = await userRepository.Get(player1UserName);
            if (user1 == null)
                throw new ApplicationException($"Invalid player 1 [{player1UserName}]");

            var user2 = await userRepository.Get(player2UserName);
            if (user2 == null)
                throw new ApplicationException($"Invalid player 2 [{player2UserName}]");

            return await CreateGame(language, boardId ,user1, user2);
        }
        private async Task<IGame> CreateGame(string language, int boardId ,IUser player01, IUser player02)
        {
            var lexicon = await lexiconService.GetDictionary(language);
            if (lexicon == null)
                throw new ApplicationException($"Invalid language [{language}]");
            var board = await GetBoard(boardId);
            if (board == null)
                throw new ApplicationException($"Invalid board [{boardId}]");


            var p1Details = await userRepository.GetDetails(player01.UserName);
            var p2Details = await userRepository.GetDetails(player02.UserName);

            var letterbag = new LetterBag(language);
            var game = new Game
            {
                Id = Guid.NewGuid().ToString().Replace("-", "").ToUpper(),
                Language = language,
                CreationDate = DateTime.UtcNow,
                Board = board,
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
                CurrentStart = DateTime.UtcNow,
                AvailableLetters = lexicon.Language.GetLettersOnly()
            };            
            return game;
        }

        private async Task<IGame> StartGame(IGameQueue queue1, IGameQueue queue2 = null)
        {            
            var player1 = queue1.Player1;
            var player2 = queue1.Player2;
                        
            if(queue2 != null)  // Queue Game Match
            {
                if (queue1.Player2 != null || queue2.Player2 != null)
                    throw new ApplicationException("A Matched game cannot have player 2 on both queues");
                player2 = queue2.Player1;
            }

            var game = await NewMultiPlayerGame(queue1.Language, queue1.BoardId, player1, player2);
            // Save game to DB
            await gameRepository.Add(game.ToDataModel());
            gameCache.AddGame(game);

            logger.Info($"Game Started = P1:{player1} P2:{player2} Language:{queue1.Language} Board:{queue1.BoardId}", context: game.Id);
            return game;
        }
        private async Task<GameResult> FinishGame(string gameId, FinishReason reason, string forfeitPlayer = null)
        {

            // Todo end game logic
            var game = gameCache.ActiveGames.SingleOrDefault(x => x.Id == gameId)?.ToDto<Game>();
            if (game == null)
                throw new ArgumentException("Invalid game id");

            game.Status = GameStatus.Finished;
            game.FinishReason = reason;
            game.FinishDate = DateTime.UtcNow;

            game.P1FinalScore = game.PlayMoves
                    .Where(m => m.Player == game.Player01.UserName)
                    .Sum(m => m.Score) - game.Player01.Rack.Sum(l => l.Char.LetterValue(game.Language));
            if (game.P1FinalScore < 0)
                game.P1FinalScore = 0;

            game.P2FinalScore = game.PlayMoves
                .Where(m => m.Player == game.Player02.UserName)
                .Sum(m => m.Score) - game.Player02.Rack.Sum(l => l.Char.LetterValue(game.Language));
            if (game.P2FinalScore < 0)
                game.P2FinalScore = 0;

            IGamePlayer winner;
            IGamePlayer looser;
            if (reason == FinishReason.Forfeit)
            {
                looser = game.GetPlayer(forfeitPlayer);
                if (game == null)
                    throw new ApplicationException("Forfeit must have a forfeit player");
                winner = game.Player01.UserName == looser.UserName
                    ? game.Player02
                    : game.Player01;
            }
            else
            {
                if (game.P1FinalScore > game.P2FinalScore)
                {
                    winner = game.Player01;
                    looser = game.Player02;
                }
                else if (game.P2FinalScore > game.P1FinalScore)
                {
                    winner = game.Player02;
                    looser = game.Player01;
                }
                else
                {
                    winner = null;
                    looser = null;
                }
            }
            game.Winner = winner?.UserName;            

            // todo save game to db
            await gameRepository.Update(game.ToDataModel());
                        
            gameCache.RemoveGame(game.Id);
            await logger.InfoAsync($"GAME OVER! Winner:[{game.Winner}] Duration:[{Math.Round((game.FinishDate.Value-game.CreationDate).TotalMinutes,2)}]", context:game.Id);

            var res = new GameResult
            {
                GameId = game.Id,
                Player1 = game.Player01.UserName,
                Player2 = game.Player02.UserName,
                Winner = winner?.UserName,
                Duration = Math.Round((game.FinishDate.Value - game.CreationDate).TotalMinutes, 2),
                P1Score = game.PlayMoves.Where(m => m.Player == game.Player01.UserName).Sum(m => m.Score),
                P2Score = game.PlayMoves.Where(m => m.Player == game.Player02.UserName).Sum(m => m.Score),
                P1PlayCount = game.PlayMoves.Where(m => m.Player == game.Player01.UserName).Count(),
                P2PlayCount = game.PlayMoves.Where(m => m.Player == game.Player02.UserName).Count(),
                Reason = reason
            };
            res.P1Average = res.P1PlayCount == 0 ? 0 : res.P1Score / res.P1PlayCount;
            res.P2Average = res.P2PlayCount == 0 ? 0 : res.P2Score / res.P2PlayCount;

            statService.UpdateStats(game.Id);

            return res;
        }

        private async Task<IBoard> GetBoard(int boardId)
        {
            var boards = await boardRepository.List();
            var selected = boards.SingleOrDefault(b => b.Id == boardId);
            if (selected == null)
                throw new Exception($"Ivalid board Id {boardId}");
            return await boardRepository.Get(selected.Id);
        }
        private async Task<IBoard> GetDefaultBoard()
        {
            var boards = await boardRepository.List();
            var selected = boards.SingleOrDefault(b => b.Name == "Standard");            
            return await boardRepository.Get(selected.Id);
        }
        

        private IUser GetAI(int aiLevel)
        {
            throw new NotImplementedException();
        }

        
        
                
        #endregion

    }
}
