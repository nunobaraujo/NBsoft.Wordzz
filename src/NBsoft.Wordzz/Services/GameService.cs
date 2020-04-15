using NBsoft.Wordzz.Contracts;
using NBsoft.Wordzz.Contracts.Entities;
using NBsoft.Wordzz.Core.Models;
using NBsoft.Wordzz.Core.Repositories;
using NBsoft.Wordzz.Core.Services;
using NBsoft.Wordzz.Extensions;
using NBsoft.Wordzz.Helpers;
using NBsoft.Wordzz.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NBsoft.Wordzz.Services
{
    internal class GameService : IGameService
    {
        private readonly IUserRepository _userRepository;
        private readonly IWordRepository _wordRepository;
        public GameService(IUserRepository userRepository, IWordRepository wordRepository)
        {
            _userRepository = userRepository;
            _wordRepository = wordRepository;
        }

        public IBoard GenerateBoard(int size)
        {
            var board = new Board();
            board.Rows = size;
            board.Columns = size;
            board.Words = new List<IBoardWord>();

            var tiles = new List<BoardTile>();
            for (int x = 1; x <= board.Rows; x++)
            {
                for (int y = 1; y <= board.Columns; y++)
                {
                    tiles.Add(new BoardTile { X = x, Y = y, Bonus = BonusType.Regular });
                }
            }
            board.Tiles = tiles.Select(x => (IBoardTile)x).ToArray();

            var newBoard = board.ApplyBonusTiles();

            return newBoard; ;
        }


        public async Task<IGame> NewGame(string language, string player1UserName, string player2UserName, int size)
        {        
            var user1 = await _userRepository.Get(player1UserName);
            if (user1 == null)
                throw new ApplicationException($"Invalid player 1 [{player1UserName}]");

            var user2 = await _userRepository.Get(player2UserName);
            if (user2 == null)
                throw new ApplicationException($"Invalid player 2 [{player2UserName}]");

            return await NewGame(language, user1, user2, size);
        }

        public async Task<IGame> NewGame(string language, string player1UserName, int aiLevel, int size)
        {
            var user1 = await _userRepository.Get(player1UserName);
            if (user1 == null)
                throw new ApplicationException($"Invalid player 1 [{player1UserName}]");

            return await NewGame(language, user1, GetAI(aiLevel), size);

        }
       
        private async Task<IGame> NewGame(string language, IUser player01, IUser player02, int size)
        {
            var lexicon = await _wordRepository.GetDictionary(language);
            if (lexicon == null)
                throw new ApplicationException($"Invalid language [{language}]");
            var game = new Game
            {
                Id = Guid.NewGuid().ToString(),
                Language = language,
                Board = GenerateBoard(size),
                LetterBag = LetterBagHelper.GenereateLetterBag(language),
                Player01 = new GamePlayer
                {
                    Player = player01.UserName,
                    History = new List<IPlayTurn>(),
                    Rack = new List<ILetter>()
                },
                Player02 = new GamePlayer
                {
                    Player = player02.UserName,
                    History = new List<IPlayTurn>(),
                    Rack = new List<ILetter>()
                },
                CurrentPlayer = player01.UserName,
                Status = GameStatus.Ongoing,
                CurrentStart = DateTime.UtcNow
            };

            GameHandler.ActiveGames.Add(game.Id, game);
            return game;
        }

        private IUser GetAI(int aiLevel)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<string>> GetContacts(string userId) => _userRepository.GetContacts(userId);
    }
}
