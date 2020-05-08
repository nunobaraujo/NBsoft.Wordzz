using NBsoft.Logs;
using NBsoft.Logs.Interfaces;
using NBsoft.Wordzz.Contracts;
using NBsoft.Wordzz.Contracts.Entities;
using NBsoft.Wordzz.Core.Repositories;
using NBsoft.Wordzz.Core.Services;
using NBsoft.Wordzz.Entities;
using NBsoft.Wordzz.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NBsoft.Wordzz.Services
{
    internal class StatService : IStatService
    {
        private const int TimerRepetition = 120;//seconds

        private static object arrayLock = new object();

        private readonly ILogger log;
        private readonly IStatsRepository statsRepository;
        private readonly IGameRepository gameRepository;

        private readonly List<string> gamesToUpdate;
        private readonly List<string> usersUpdated;
        private readonly Timer timer;

        private bool checkInProgress = false;

        public StatService(ILogger log, IStatsRepository statsRepository, IGameRepository gameRepository)
        {
            this.log = log;
            this.statsRepository = statsRepository;
            this.gameRepository = gameRepository;

            gamesToUpdate = new List<string>();
            usersUpdated = new List<string>();
            timer = new Timer(new TimerCallback(StatsTimerCallback));
            timer.Change(1000 * TimerRepetition, -1);            

            this.log.Info("StatService Started");
        }

        public async Task<IUserStats> GetStats(string userName)
        {
            var userStats = await statsRepository.Get(userName);
            return userStats;
        }

        public void UpdateStats(string gameId)
        {
            var alreadyMarked = gamesToUpdate.SingleOrDefault(g => g == gameId);
            if (alreadyMarked != null)
                return;
            lock (arrayLock)
            {
                gamesToUpdate.Add(gameId);
            }   
            if (!checkInProgress)
                timer.Change(100, -1);

        }

        private void StatsTimerCallback(object state)
        {
            if (checkInProgress)
                return;
            checkInProgress = true;            
            if (gamesToUpdate.Count() > 0)
            {
                string[] arrayCopy;
                lock (arrayLock)
                {
                    arrayCopy = new string[gamesToUpdate.Count()];
                    Array.Copy(gamesToUpdate.ToArray(), arrayCopy, arrayCopy.Length);
                    gamesToUpdate.Clear();
                }
                usersUpdated.Clear();
                foreach (var gameId in arrayCopy)
                {
                    Task.Run(async () => await UpdateGameStats(gameId)).Wait();
                }
            }

            checkInProgress = false;            
            timer.Change(1000 * TimerRepetition, -1);
        }

        private async Task UpdateGameStats(string gameId)
        {
            try
            {
                var game = await gameRepository.Get(gameId);
                // Player 1
                if (!usersUpdated.Contains(game.Player01))
                {
                    var updated = await UpdateUserStats(game.Player01);
                    if (updated)
                        usersUpdated.Add(game.Player01);
                }
                // Player 2
                if (!usersUpdated.Contains(game.Player02))
                {
                    var updated = await UpdateUserStats(game.Player02);
                    if (updated)
                        usersUpdated.Add(game.Player02);
                }
            }
            catch (Exception ex) 
            {
                await log.ErrorAsync($"Error updating Game Stats for {gameId}", ex);
            }
        }

        private async Task<bool> UpdateUserStats(string username)
        {
            if (string.IsNullOrEmpty(username))
                return false;
            
            var uGames = await gameRepository.GetByUser(username);
            if (uGames.Count() < 1)
                return false;            
            var uStats = await statsRepository.Get(username);
            var uMoves = await gameRepository.GetMovesByUser(username);


            // Highscore game
            var highScoreGame = GetHighScoreGame(uGames, username, out string highScoreGameOpponent);

            // Highscore play
            var highScorePlay = GetHighScoreMove(uGames, uMoves, username, out string highScorePlayOpponent);

            // HighScore Word
            var highScoreWord = GetHighScoreWord(uGames, uMoves, username, out string highScoreWordOpponent, out string highScoreWordName);
                        

            var newStats = new UserStats
            {
                UserName = username,
                Victories = GetVictories(uGames, username),
                Defeats = GetDefeats(uGames,username),
                Draws = GetDraws(uGames),
                GamesPlayed = GetGamesPlayed(uGames),
                TotalScore = GetTotalScore(uGames, username),
                HighScoreGame = highScoreGame,
                HighScoreGameOpponent = highScoreGameOpponent,
                HighScorePlay = highScorePlay,
                HighScorePlayOpponent = highScorePlayOpponent,
                HighScoreWord = highScoreWord,
                HighScoreWordName = highScoreWordName,
                HighScoreWordOpponent = highScoreWordOpponent,
                MostUsedWord = GetMostUsedWord(uMoves),
                MostFrequentOpponent = GetMostFrequentOpponent(uGames, username),
                Forfeits = GetForfeits(uGames, username)
            };

            if (uStats == null)
                await statsRepository.Add(newStats);
            else
                await statsRepository.Update(newStats);


            log.Info($"User stats updated: {newStats.ToJson()}",context: username);
            return true;
        }

        private uint GetVictories(IEnumerable<GameDataModel> games, string userName) => (uint)games.Where(g => g.Winner == userName).Count();
        private uint GetDefeats(IEnumerable<GameDataModel> games, string userName) => (uint)games.Where(g => g.Winner != null && g.Winner != userName).Count();
        private uint GetDraws(IEnumerable<GameDataModel> games) => (uint)games.Where(g => g.Status == (int)GameStatus.Finished && g.Winner == null).Count();
        private uint GetGamesPlayed(IEnumerable<GameDataModel> games) => (uint)games.Count();
        private uint GetForfeits(IEnumerable<GameDataModel> games, string userName) => (uint)games.Where(g => g.FinishReason == (int)FinishReason.Forfeit && g.Winner != userName).Count();

        private string GetMostUsedWord(IEnumerable<GameMoveDataModel> moves) 
        {
            if (moves == null || moves.Count() < 1)
                return "";

            var processedWords = moves.Select(m => new WordModel
            {
                MoveId = m.Id,
                Words = m.Words.FromJson<PlayWord[]>()
            });

            var allWords = processedWords.SelectMany(w => w.Words);
            if (allWords.Count() < 1)
                return "";

            Dictionary<string, int> wordCount = new Dictionary<string, int>();
            foreach (var word in allWords)
            {
                var wName = word.GetString();
                if (!wordCount.ContainsKey(wName))
                {
                    wordCount.Add(wName, 0);
                }
                var value = wordCount[wName];                ;
                wordCount[wName] = ++value;
            }

            var topWord = wordCount.FirstOrDefault(d => d.Value == wordCount.Max(w => w.Value));
            return topWord.Key;
        }
        private string GetMostFrequentOpponent(IEnumerable<GameDataModel> games, string userName)
        {
            if (games == null || games.Count() < 1)
                return "";

            var p1Opponents = games.Where(g => g.Player01 != userName).Select(g => g.Player01);
            var p2Opponents = games.Where(g => g.Player02 != userName).Select(g => g.Player02);

            var all = p1Opponents.Concat(p2Opponents);
            var groups = all.GroupBy(o => o);

            int topCount = 0;
            string opponent = null;
            foreach (var group in groups)
            {
                if (group.Count() >= topCount) 
                {
                    topCount = group.Count();
                    opponent = group.Key;
                }
            }
            return opponent;
        }

        private uint GetTotalScore(IEnumerable<GameDataModel> games, string userName)
        {
            if (games == null || games.Count() < 1)
                return 0;            

            var p1Total = games.Where(g => g.Player01 == userName).Sum(g => g.P1FinalScore);
            var p2Total = games.Where(g => g.Player02 == userName).Sum(g => g.P2FinalScore);

            var result = p1Total + p2Total;
            if (result < 0)
                result = 0;
            return (uint)result;
        }
        private uint GetHighScoreGame(IEnumerable<GameDataModel> games, string userName, out string opponent)
        {
            if (games == null || games.Count() < 1)
            {
                opponent = "";
                return 0;
            }
            var p1Games = games.Where(g => g.Player01 == userName);
            var p2Games = games.Where(g => g.Player02 == userName);

            int highScoreP1 = 0;
            int highScoreP2 = 0;

            if (p1Games != null && p1Games.Count() > 0)
                highScoreP1 = p1Games.Max(g => g.P1FinalScore);
            if (p2Games != null && p2Games.Count() > 0)
                highScoreP2 = p2Games.Max(g => g.P2FinalScore);

            uint highScore = 0;
            
            if (highScoreP1 > highScoreP2)
            {
                var highScoreGame = games.FirstOrDefault(g => g.Player01 == userName && g.P1FinalScore == highScoreP1);
                highScore = (uint)highScoreP1;
                opponent = highScoreGame?.Player02;
            }
            else
            {
                var highScoreGame = games.FirstOrDefault(g => g.Player02 == userName && g.P2FinalScore == highScoreP2);
                highScore = (uint)highScoreP2;
                opponent = highScoreGame?.Player01;
            }
            return highScore;
        }
        private uint GetHighScoreMove(IEnumerable<GameDataModel> games, IEnumerable<GameMoveDataModel> moves, string userName, out string opponent)
        {
            if (games == null || games.Count() < 1 || moves == null || moves.Count() < 1)
            {
                opponent = "";
                return 0;
            }

            uint highScorePlay = (uint)moves.Max(m => m.Score);
            var highScoreMove = moves.First(m => m.Score == highScorePlay);
            var highScoreMoveGame = games.Single(g => g.Id == highScoreMove.GameId);
            opponent = highScoreMoveGame.Player01 == userName 
                ? highScoreMoveGame.Player02 
                : highScoreMoveGame.Player01;

            return highScorePlay;
        }
        private uint GetHighScoreWord(IEnumerable<GameDataModel> games, IEnumerable<GameMoveDataModel> moves, string userName, out string opponent, out string word)
        {
            if (games == null || games.Count() < 1 || moves == null || moves.Count() < 1)
            {
                word = "";
                opponent = "";
                return 0;
            }

            word = null;
            opponent = null;
            
            var processedWords = moves.Select(m => new WordModel
            {
                MoveId = m.Id,
                Words = m.Words.FromJson<PlayWord[]>()
            });

            uint hMoveId = 0;
            int hWordMoveScore = 0;
            PlayWord hWord = null;
            foreach (var pWord in processedWords)
            {
                if (pWord.Words!= null && pWord.Words.Length > 0)
                {
                    var maxScore = pWord.Words.Max(w => w.Score);
                    if (maxScore >= hWordMoveScore)
                    {
                        hMoveId = pWord.MoveId;
                        hWordMoveScore = maxScore;
                        hWord = pWord.Words.First(w => w.Score == maxScore);
                    }
                }
            }            

            var gameId = moves.SingleOrDefault(m => m.Id == hMoveId)?.GameId;
            if (!string.IsNullOrEmpty(gameId))
            {
                word = hWord.GetString();
                var game = games.Single(g => g.Id == gameId);
                opponent = game.Player01 == userName
                    ? game.Player02
                    : game.Player01;

            }

            return (uint)hWordMoveScore;

        }

        

        private class WordModel
        {
            public uint MoveId {get;set;}
            public PlayWord[] Words { get; set; }

        }
    }
}
