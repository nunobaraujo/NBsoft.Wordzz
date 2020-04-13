using NBsoft.Wordzz.Contracts;
using NBsoft.Wordzz.Contracts.Entities;
using NBsoft.Wordzz.Core.Repositories;
using NBsoft.Wordzz.Core.Services;
using NBsoft.Wordzz.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NBsoft.Wordzz.Services
{
    internal class GameService : IGameService
    {
        private readonly IUserRepository _userRepository;
        public GameService(IUserRepository userRepository)
        {
            _userRepository = userRepository;

        }
        public IBoard GenerateBoard(int rows, int columns)
        {
            var board = new Board();
            board.Rows = rows;
            board.Columns = columns;
            board.Words = new List<IBoardWord>();

            var tiles = new List<BoardTile>();
            for (int x = 1; x <= rows; x++)
            {
                for (int y = 1; y <= 15; y++)
                {
                    tiles.Add(new BoardTile { X = x , Y = y , Bonus = BonusType.Regular });
                }
            }
            board.Tiles = tiles.Select(x => (IBoardTile)x).ToArray();

            board = ApplyBonusTiles(board);

            return board;
        }

        public async Task<IGame> NewGame(string player1UserName, string player2UserName, int rows, int columns)
        {
            var user1 = await _userRepository.Get(player1UserName);
            var user2 = _userRepository.Get(player2UserName);

            if (user1 == null)
                throw new ApplicationException($"Invalid player 1 [{player1UserName}]");
            if (user2 == null)
                throw new ApplicationException($"Invalid player 2 [{player2UserName}]");

            var p1Bag = GenerateLetterBag

            var game = new Game
            {
                Board = GenerateBoard(rows, columns),
                Player01 = new GamePlayer
                { 
                    Player = user1.UserName,
                    History = new List<IPlayTurn> (),
                    LetterBag = 
                },

                
                CurrentPlayer = user1.UserName,
                

            };
            return game;
        }

        public async Task<IGame> NewGame(string player1UserName, int aiLevel, int rows, int columns)
        {
            throw new NotImplementedException();
        }

        private Board ApplyBonusTiles(Board board)
        {
            if (board.Rows == 15 && board.Columns == 15)
            {
                // Center
                board.Tile(8, 8).Bonus = BonusType.Center;

                // TripleWord
                board.Tile(1, 1).Bonus = BonusType.TripleWord;
                board.Tile(1, 8).Bonus = BonusType.TripleWord;
                board.Tile(1, 15).Bonus = BonusType.TripleWord;

                board.Tile(8, 1).Bonus = BonusType.TripleWord;
                board.Tile(8, 15).Bonus = BonusType.TripleWord;

                board.Tile(15, 1).Bonus = BonusType.TripleWord;
                board.Tile(15, 8).Bonus = BonusType.TripleWord;
                board.Tile(15, 15).Bonus = BonusType.TripleWord;

                // DoubleWord
                board.Tile(2, 2).Bonus = BonusType.DoubleWord;
                board.Tile(2, 14).Bonus = BonusType.DoubleWord;

                board.Tile(3, 3).Bonus = BonusType.DoubleWord;
                board.Tile(3, 13).Bonus = BonusType.DoubleWord;

                board.Tile(4, 4).Bonus = BonusType.DoubleWord;
                board.Tile(4, 12).Bonus = BonusType.DoubleWord;

                board.Tile(5, 5).Bonus = BonusType.DoubleWord;
                board.Tile(5, 11).Bonus = BonusType.DoubleWord;

                board.Tile(11, 5).Bonus = BonusType.DoubleWord;
                board.Tile(11, 11).Bonus = BonusType.DoubleWord;

                board.Tile(12, 4).Bonus = BonusType.DoubleWord;
                board.Tile(12, 12).Bonus = BonusType.DoubleWord;

                board.Tile(13, 3).Bonus = BonusType.DoubleWord;
                board.Tile(13, 13).Bonus = BonusType.DoubleWord;

                board.Tile(14, 2).Bonus = BonusType.DoubleWord;
                board.Tile(14, 14).Bonus = BonusType.DoubleWord;


                // TripleLetter
                board.Tile(2, 6).Bonus = BonusType.TripleLetter;
                board.Tile(2, 10).Bonus = BonusType.TripleLetter;

                board.Tile(6, 2).Bonus = BonusType.TripleLetter;
                board.Tile(6, 6).Bonus = BonusType.TripleLetter;
                board.Tile(6, 10).Bonus = BonusType.TripleLetter;
                board.Tile(6, 14).Bonus = BonusType.TripleLetter;

                board.Tile(10, 2).Bonus = BonusType.TripleLetter;
                board.Tile(10, 6).Bonus = BonusType.TripleLetter;
                board.Tile(10, 10).Bonus = BonusType.TripleLetter;
                board.Tile(10, 14).Bonus = BonusType.TripleLetter;

                board.Tile(14, 6).Bonus = BonusType.TripleLetter;
                board.Tile(14, 10).Bonus = BonusType.TripleLetter;

                // DoubleLetter
                board.Tile(1, 4).Bonus = BonusType.DoubleLetter;
                board.Tile(1, 12).Bonus = BonusType.DoubleLetter;

                board.Tile(3, 7).Bonus = BonusType.DoubleLetter;
                board.Tile(3, 9).Bonus = BonusType.DoubleLetter;

                board.Tile(4, 1).Bonus = BonusType.DoubleLetter;
                board.Tile(4, 8).Bonus = BonusType.DoubleLetter;
                board.Tile(4, 15).Bonus = BonusType.DoubleLetter;

                board.Tile(7, 3).Bonus = BonusType.DoubleLetter;
                board.Tile(7, 7).Bonus = BonusType.DoubleLetter;
                board.Tile(7, 9).Bonus = BonusType.DoubleLetter;
                board.Tile(7, 13).Bonus = BonusType.DoubleLetter;

                board.Tile(8, 4).Bonus = BonusType.DoubleLetter;
                board.Tile(8, 12).Bonus = BonusType.DoubleLetter;

                board.Tile(9, 3).Bonus = BonusType.DoubleLetter;
                board.Tile(9, 7).Bonus = BonusType.DoubleLetter;
                board.Tile(9, 9).Bonus = BonusType.DoubleLetter;
                board.Tile(9, 13).Bonus = BonusType.DoubleLetter;

                board.Tile(12, 1).Bonus = BonusType.DoubleLetter;
                board.Tile(12, 8).Bonus = BonusType.DoubleLetter;
                board.Tile(12, 15).Bonus = BonusType.DoubleLetter;

                board.Tile(13, 7).Bonus = BonusType.DoubleLetter;
                board.Tile(13, 9).Bonus = BonusType.DoubleLetter;

                board.Tile(15, 4).Bonus = BonusType.DoubleLetter;
                board.Tile(15, 12).Bonus = BonusType.DoubleLetter;
            }
            return board;
        }
    }
}
