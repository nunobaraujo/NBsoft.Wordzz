using NBsoft.Wordzz.Contracts;
using NBsoft.Wordzz.Contracts.Entities;
using NBsoft.Wordzz.Core.Services;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBsoft.Wordzz.Extensions
{
    public static class WordExtensions
    {
        public static string GetString(this IPlayWord src)
        {
            if (src == null)
                return null;
            return src.Letters.GetString();
        }
        public static string GetString(this IEnumerable<IPlayLetter> src)
        {
            var letters = src.Select(l => l.Letter.Letter);
            return letters.GetString();
        }
        public static string GetString(this IEnumerable<ILetter> src)
        {
            var chars = src.Select(l => l.Char);
            var sb = new StringBuilder(chars.Count());
            foreach (var c in chars)
            {
                sb.Append(c);
            }
            return sb.ToString();
        }

        public static IEnumerable<ILetter> GetLetters(this string src, string language)
        {
            var result = new List<ILetter>();
            foreach (var c in src)
            {
                var letter = new Letter
                {
                    Char = c,
                    IsBlank = c == ' ',
                    Value = c.LetterValue(language)
                };
                result.Add(letter);
            }
            return result;
        }


        public static int CalculateRawScore(this IEnumerable<IPlayLetter> src, string language) {
            if (src == null)
                return 0;
            return src
                .Where(l => l.Letter.Letter.IsBlank == false)
                .Sum(l => l.Letter.Letter.Char.LetterValue(language));
        }
        public static int CalculateEffectiveScore(this IEnumerable<IPlayLetter> src, string language, IEnumerable<IPlayLetter> playedLetters)
        {
            if (src == null)
                return 0;

            int score = 0;
            int tripleWordCount = 0;
            int doubleWordCount = 0;

            foreach (var letter in src)
            {
                var inCurrentPlay = playedLetters.SingleOrDefault(l => l.Tile.X == letter.Tile.X && l.Tile.Y == letter.Tile.Y);
                var isOwned = inCurrentPlay != null;

                var letterRawValue = letter.Letter.Letter.Char.LetterValue(language);
                if (letter.Letter.Letter.IsBlank)
                    letterRawValue = 0;

                switch (letter.Tile.Bonus)
                {
                    case BonusType.Regular:
                    case BonusType.Center:
                        score += letterRawValue;
                        break;
                    case BonusType.DoubleLetter:
                        score += isOwned 
                                ? letterRawValue * 2 
                                : letterRawValue;
                        break;
                    case BonusType.TripleLetter:
                        score += isOwned
                                ? letterRawValue * 3
                                : letterRawValue;
                        break;
                    case BonusType.DoubleWord:
                        score += letterRawValue;
                        if (isOwned)
                            doubleWordCount++;
                        break;
                    case BonusType.TripleWord:
                        score += letterRawValue;
                        if (isOwned)
                            tripleWordCount++;
                        break;
                    default:
                        return -1;
                }
            }

            if (doubleWordCount > 0)
                score = score * 2 * doubleWordCount;
            if (tripleWordCount > 0)
                score = score * 3 * tripleWordCount;
            return score;
        }

        public static int GetStartX(this IPlayWord src) => src.Letters.GetStartX();
        public static int GetEndX(this IPlayWord src)=> src.Letters.GetEndX();        
        
        public static int GetStartY(this IPlayWord src)=> src.Letters.GetStartY();
        public static int GetEndY(this IPlayWord src) => src.Letters.GetEndY();


        public static int GetStartX(this IEnumerable<IPlayLetter> src)
        {
            if (src == null || src.Count() < 1)
                return -1;
            return src.Min(l => l.Tile.X);
        }
        public static int GetEndX(this IEnumerable<IPlayLetter> src) 
        {
            if (src == null || src.Count() < 1)
                return -1;
            return src.Max(l => l.Tile.X);
        }
        
        public static int GetStartY(this IEnumerable<IPlayLetter> src)
        {
            if (src == null || src.Count() < 1)
                return -1;
            return src.Min(l => l.Tile.Y);
        }
        public static int GetEndY(this IEnumerable<IPlayLetter> src)
        {
            if (src == null || src.Count() < 1)
                return -1;
            return src.Max(l => l.Tile.Y);
        }
                        

    }
}
