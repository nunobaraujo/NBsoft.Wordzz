using NBsoft.Wordzz.Contracts;
using NBsoft.Wordzz.Contracts.Entities;
using NBsoft.Wordzz.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;


namespace NBsoft.Wordzz.GameLogic
{
    public static class BoardExtensions
    {
        public static BoardTile Tile(this Board src, int x, int y)
        {
            return src.Tiles.SingleOrDefault(t => t.X == x && t.Y == y);
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
            var outOfBounds = playedLetters.Where(l => l.Tile.X < 1 || l.Tile.X > board.BoardColumns || l.Tile.Y < 1 || l.Tile.Y > board.BoardRows);
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
        public static string CheckWordStructure(this IBoard board, IEnumerable<IPlayLetter> existingLetters, IEnumerable<IPlayLetter> playedLetters)
        {
            // Validate out of bounds
            var outOfBounds = playedLetters.Where(l => l.Tile.X < 1 || l.Tile.X > board.BoardColumns || l.Tile.Y < 1 || l.Tile.Y > board.BoardRows);
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
            var outOfBounds = playedLetters.Where(l => l.Tile.X < 1 || l.Tile.X > board.BoardColumns || l.Tile.Y < 1 || l.Tile.Y > board.BoardRows);
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
                    if (tile.x >= 1 && tile.x <= board.BoardColumns && tile.y >= 1 && tile.y <= board.BoardRows)
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

        public static PlayWord GetHorizontalWord(this IBoard board, IEnumerable<IPlayLetter> allLetters, IPlayLetter letter)
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
            while (endX <= board.BoardColumns)
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
            return new PlayWord
            {
                Letters = word.Select(l => l.ToDto<PlayLetter>()).ToArray(),
                Score = 0,
                RawScore = 0,
            };
        }
        public static PlayWord GetVerticalWord(this IBoard board, IEnumerable<IPlayLetter> allLetters, IPlayLetter letter)
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
            while (endY <= board.BoardRows)
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
                Letters = word.Select(l => l.ToDto<PlayLetter>()).ToArray(),
                Score = 0,
                RawScore = 0,
            };
        }

    }
}
