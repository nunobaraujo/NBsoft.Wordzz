using NBsoft.Wordzz.Contracts;
using NBsoft.Wordzz.Contracts.Entities;
using NBsoft.Wordzz.Core.Services;
using NBsoft.Wordzz.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBsoft.Wordzz.Extensions
{
    public static class GameExtensions
    {
        public static async Task<MoveCheck> ValidateMove(this Game game, PlayLetter[] letters, ILexiconService lexiconService)
        {

            if (letters.Length < 1)
                return new MoveCheck { Result = "No letters." };
                        
            var existingLetters = game.PlayMoves.SelectMany(x => x.Letters);            
            
            // Check if new letters are out of bounds or overlap existing letters            
            string positionCheck = game.Board.CheckTilePositions(existingLetters, letters);
            if (positionCheck != "OK")
                return new MoveCheck { Result = positionCheck };
                        
            // Check word structure
            string structureCheck = game.Board.CheckWordStructure(existingLetters, letters);
            if (structureCheck != "OK")
                return new MoveCheck { Result = structureCheck };

            // Check word position
            string wordPositionCheck = game.Board.CheckWordPosition(existingLetters, letters);
            if (wordPositionCheck != "OK")
                return new MoveCheck { Result = wordPositionCheck };

            // Extract all possible words
            var words = await game.ExtractAllWords(lexiconService, existingLetters, letters);
            if (words == null || words.Count() < 1)
            {
                return new MoveCheck { Result = "Invalid words" };
            }
            
            var result = new MoveCheck 
            { 
                Result = "OK",
                Words = words
            };
            return result;
        }
        /// <summary>
        /// Check OUT OF BOUNDS and OVERLLAPING letters
        /// </summary>
        /// <param name="board"></param>
        /// <param name="existingLetters"></param>
        /// <param name="playedLetters"></param>
        /// <returns></returns>
        public static string CheckTilePositions(this IBoard board, IEnumerable<IPlayLetter> existingLetters, IEnumerable<IPlayLetter> playedLetters)
        {
            var outOfBounds = playedLetters.Where(l => l.Tile.X < 1 || l.Tile.X > board.Columns || l.Tile.Y < 1 || l.Tile.Y > board.Rows);
            if (outOfBounds.Count() > 0)
                return $"Letter(s) out of bounds: {string.Join(",", outOfBounds.Select(t => t.Letter.Letter.Char))}";

            var existingCoordinates = existingLetters.Select(x => $"{x.Tile.X}-{x.Tile.Y}");
            var newCoordinates = playedLetters.Select(x => $"{x.Tile.X}-{x.Tile.Y}");
            var overlapped = newCoordinates.Where(c => existingCoordinates.Contains(c));
            if (overlapped.Count() > 0)
                return $"Letters(s) overlapped: {string.Join(",", overlapped)}";            
            return "OK";
        }

        /// <summary>
        /// Check if all letters are in the SAME AXIX 
        /// and for EMPTY SPACES between letters 
        /// </summary>
        /// <param name="board"></param>
        /// <param name="existingLetters"></param>
        /// <param name="playedLetters"></param>
        /// <returns></returns>
        public static string CheckWordStructure(this IBoard board, IEnumerable<IPlayLetter> existingLetters, IEnumerable<IPlayLetter>  playedLetters)
        {
            // Validate out of bounds
            var outOfBounds = playedLetters.Where(l => l.Tile.X < 1 || l.Tile.X > board.Columns || l.Tile.Y < 1 || l.Tile.Y > board.Rows);
            if (outOfBounds.Count() > 0)
            {
                var first = outOfBounds.First();
                var message = $"Out of bounds: {first.Letter.Letter.Char}={first.Tile.X}-{first.Tile.Y}";
                throw new IndexOutOfRangeException(message);
            }

            char axis = '_';
            int fixedPos = -1;
            var allLetters = new List<IPlayLetter>();
            allLetters.AddRange(existingLetters);
            allLetters.AddRange(playedLetters);            
            
            // Check if all letters are in the same axis
            if (playedLetters.Select(l => l.Tile.X).Distinct().Count() == 1)
            {
                fixedPos = playedLetters.Select(l => l.Tile.X).Distinct().First();
                axis = 'v'; // Word is Vertical
            }
            if (playedLetters.Select(l => l.Tile.Y).Distinct().Count() == 1)
            {
                fixedPos = playedLetters.Select(l => l.Tile.Y).Distinct().First();
                axis = 'h'; // Word is Horizontal
            }

            switch (axis)
            {
                default:
                    return "Letters in more than one axis.";
                case 'h':
                    // Check for empty spaces Horizontaly
                    var minX = playedLetters.GetStartX();
                    var maxX = playedLetters.GetEndX();
                    for (int x = minX; x <= maxX; x++)
                    {
                        var tile = allLetters.SingleOrDefault(l => l.Tile.X == x && l.Tile.Y == fixedPos);
                        if (tile == null)
                            return "Empty space.";
                    }
                    break;
                case 'v':
                    // Check for empty spaces Verticaly
                    var minY = playedLetters.GetStartY();
                    var maxY = playedLetters.GetEndY();
                    for (int y = minY; y <= maxY; y++)
                    {
                        var tile = allLetters.SingleOrDefault(l => l.Tile.Y == y && l.Tile.X == fixedPos);
                        if (tile == null)
                            return "Empty space.";
                    }
                    break;                
            }
            return "OK";
        }

        /// <summary>
        /// Checks if word is CONNECTED to EXISTING WORD 
        /// or if it is the first word check if it uses the STAR TILE
        /// </summary>
        /// <param name="board"></param>
        /// <param name="existingLetters"></param>
        /// <param name="playedLetters"></param>
        /// <returns></returns>
        public static string CheckWordPosition(this IBoard board, IEnumerable<IPlayLetter> existingLetters, IEnumerable<IPlayLetter> playedLetters)
        {
            // Validate out of bounds
            var outOfBounds = playedLetters.Where(l => l.Tile.X < 1 || l.Tile.X > board.Columns || l.Tile.Y < 1 || l.Tile.Y > board.Rows);
            if (outOfBounds.Count() > 0)
            {
                var first = outOfBounds.First();
                var message = $"Out of bounds: {first.Letter.Letter.Char}={first.Tile.X}-{first.Tile.Y}";
                throw new IndexOutOfRangeException(message);
            }

            // If first word, STAR tile MUST be used
            if (existingLetters.Count() == 0)
            {
                var starTile = playedLetters.SingleOrDefault(l => l.Tile.Bonus == BonusType.Center);
                return starTile != null ? "OK" : "First word must use STAR tile";
            }

            // if not first word it needs to be connected to other word
            bool isConnected = false;
            foreach (var letter in playedLetters)
            {
                var connectedTiles = new List<dynamic>();
                connectedTiles.Add(new { x = letter.Tile.X, y = letter.Tile.Y - 1 });   // Top Tile
                connectedTiles.Add(new { x = letter.Tile.X, y = letter.Tile.Y + 1 });   // Bottom Tile
                connectedTiles.Add(new { x = letter.Tile.X - 1, y = letter.Tile.Y });   // Left Tile
                connectedTiles.Add(new { x = letter.Tile.X + 1, y = letter.Tile.Y });   // Right tile

                foreach (var tile in connectedTiles)
                {
                    if (tile.x >= 1 && tile.x <= board.Columns && tile.y >= 1 && tile.y <= board.Rows)
                    {
                        var existing = existingLetters.FirstOrDefault(l => l.Tile.X == tile.x && l.Tile.Y == tile.y);
                        if (existing != null)
                        {
                            isConnected = true;
                            break;
                        }
                    }
                }
                if (isConnected)
                    break;
            }
            return isConnected ? "OK" : "Word is not connected to existing word";
        }

        public static async Task<IEnumerable<IPlayWord>> ExtractAllWords(this IGame game, ILexiconService lexiconService, IEnumerable<IPlayLetter> existingLetters, IEnumerable<IPlayLetter> playedLetters)
        {
            var allLetters = new List<IPlayLetter>();
            allLetters.AddRange(existingLetters);
            allLetters.AddRange(playedLetters);


            var horizontalWords = new List<IPlayWord>();
            var verticalWords = new List<IPlayWord>();
            // Extract valid words
            foreach (var letter in playedLetters)
            {                
                var word = game.Board.GetHorizontalWord(allLetters, letter);
                if (word.Letters.Count() > 1)
                {
                    // check if word was already added (or it will add the main word for every checked letter)
                    var hAlreadyExists = horizontalWords
                    .SingleOrDefault(w => w.GetStartX() == word.GetStartX() && w.GetEndX() == word.GetEndX());
                    if (hAlreadyExists == null)
                    {
                        var wordString = word.GetString();
                        if (await lexiconService.ValidateWord(game.Language, wordString))
                        {
                            Console.WriteLine($"[{letter.Letter.Letter.Char}] - H word:{wordString}");
                            horizontalWords.Add(word);
                        }
                        else
                        {
                            Console.WriteLine($"[{letter.Letter.Letter.Char}] - H word Invalid:{wordString}");
                            return null; // This word is invalid
                        }
                    }
                }   
                else
                    Console.WriteLine($"[{letter.Letter.Letter.Char}] - No valid H words");


                word = game.Board.GetVerticalWord(allLetters, letter);
                if (word.Letters.Count() > 1) 
                {
                    // check if word was already added (or it will add the main word for every checked letter)                                
                    var vAlreadyExists = verticalWords
                        .SingleOrDefault(w => w.GetStartY() == word.GetStartY() && w.GetEndY() == word.GetEndY());
                    if (vAlreadyExists == null)
                    {
                        var wordString = word.GetString();                        
                        if (await lexiconService.ValidateWord(game.Language, wordString))
                        {
                            Console.WriteLine($"[{letter.Letter.Letter.Char}] - V word:{wordString}");
                            verticalWords.Add(word);
                        }
                        else
                        {
                            Console.WriteLine($"[{letter.Letter.Letter.Char}] - V word Invalid:{ wordString}");
                            return null; // This word is invalid
                        }
                    }
                }
                else
                    Console.WriteLine($"[{letter.Letter.Letter.Char}] - No valid V words");

                if (horizontalWords.Count() == 0 && verticalWords.Count() == 0)
                    return null;    // letter not in any vertical or horizontal words. Invalid move

            }
            
            return horizontalWords.Concat(verticalWords);
        }

        public static IEnumerable<IPlayWord> ScoreMove(this Game game, IEnumerable<IPlayWord> words, IEnumerable<IPlayLetter> playLetters)
        {
            var scoredWords = new List<IPlayWord>();
            foreach (var word in words)
            {
                var scored = new PlayWord
                {
                    Letters = word.Letters,
                    RawScore = word.Letters.CalculateRawScore(game.Language),
                    Score = word.Letters.CalculateEffectiveScore(game.Language, playLetters)
                };                
                scoredWords.Add(scored);
            };
            return scoredWords;
        }

        private static PlayWord GetHorizontalWord(this IBoard board, IEnumerable<IPlayLetter> allLetters, IPlayLetter letter)
        {   
            var posY = letter.Tile.Y;            
            int startX = letter.Tile.X;
            int endX = letter.Tile.X;
            
            while (startX > 0)
            {
                startX--;
                var previousLetter = allLetters.SingleOrDefault(l => l.Tile.Y == posY && l.Tile.X == startX);
                if (previousLetter == null)
                {
                    startX++;
                    break;
                }

            }
            while (endX <= board.Columns)
            {
                endX++;
                var previousLetter = allLetters.SingleOrDefault(l => l.Tile.Y == posY && l.Tile.X == endX);
                if (previousLetter == null)
                {
                    endX--;
                    break;
                }
            }

            var word = new List<IPlayLetter>();
            if (endX > startX)
            {                
                for (int x = startX; x <= endX; x++)
                {
                    var wletter = allLetters.Single(ls => ls.Tile.Y == posY && ls.Tile.X == x);
                    word.Add(wletter);
                }
            }

            // TODO check 1 letter words
            return new PlayWord {
                Letters = word.ToArray(),
                Score = 0,
                RawScore = 0,
            };
        }
        private static PlayWord GetVerticalWord(this IBoard board, IEnumerable<IPlayLetter> allLetters, IPlayLetter letter)
        {            
            int posX = letter.Tile.X;
            int startY = letter.Tile.Y;
            int endY = letter.Tile.Y;

            while (startY > 0)
            {
                startY--;
                var previousLetter = allLetters.SingleOrDefault(l => l.Tile.Y == startY && l.Tile.X == posX);
                if (previousLetter == null)
                {
                    startY++;
                    break;
                }

            }
            while (endY <= board.Rows)
            {
                endY++;
                var previousLetter = allLetters.SingleOrDefault(l => l.Tile.Y == endY && l.Tile.X == posX);
                if (previousLetter == null)
                {
                    endY--;
                    break;
                }
            }

            var word = new List<IPlayLetter>();
            if (endY > startY)
            {
                for (int y = startY; y <= endY; y++)
                {
                    var wletter = allLetters.Single(ls => ls.Tile.Y == y && ls.Tile.X == posX);
                    word.Add(wletter);
                }
            }

            // TODO check 1 letter words
            return new PlayWord
            {
                Letters = word.ToArray(),
                Score = 0,
                RawScore = 0,
            };
        }

        

    }
}
