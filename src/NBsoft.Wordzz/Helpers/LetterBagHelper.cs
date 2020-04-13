using NBsoft.Wordzz.Contracts;
using NBsoft.Wordzz.Contracts.Entities;
using NBsoft.Wordzz.Extensions;
using System;
using System.Collections.Generic;

namespace NBsoft.Wordzz.Helpers
{
    public static class LetterBagHelper
    {
        public static IEnumerable<ILetter> GenereateLetterBag(string language)
        {
            var Letters = new List<char>();
            var charList = GetLetters(language);
            foreach(char c in charList)
            {
                for (int x = 0; x < c.LetterCount(language); x++)
                {
                    Letters.Add(c);
                }
            }


            var result = new List<Letter>();

            foreach (var letter in Letters)
            {
                result.Add(new Letter { 
                    Char = letter,
                    Value = letter.LetterValue(language)
                });
            }

            return result;
        }

       

        public static char[] GetLetters(string language)
        {
            switch (language)
            {
                case "en-us":
                case "en-en":
                    return new char[]
                    {
                        'A','B','C','D','E','F','G','H','I','J','K','L',
                        'M','N','O','P','Q','R','S','T','U','V','W','X',
                        'Y','Z',' '
                    };
                case "pt-pt":
                case "pt-br":
                    return new char[]
                    {
                        'A','B','C','D','E','F','G','H','I','J','L','M',
                        'N','O','P','Q','R','S','T','U','V','X','Z','Ç',
                        ' '
                    };
                default:
                    throw new ArgumentOutOfRangeException("Invalid Language");
            }
            
        }
        public static int LetterCount(this char c, string language)
        {
            Dictionary<char, int> values;
            switch (language)
            {
                case "en-us":
                case "en-en":
                    values = new Dictionary<char, int>()
                    {
                        { ' ', 2 },
                        { 'E', 12 }, { 'A', 9 }, { 'I', 9 }, { 'O', 8 },
                        { 'N', 6 }, { 'R', 6 }, { 'T', 6 }, { 'L', 4  },
                        { 'S', 4 }, { 'U', 4 }, { 'D', 4 }, { 'G', 3 },
                        { 'B', 2 }, { 'C', 2 }, { 'M', 2 }, { 'P', 2 },
                        { 'F', 2 }, { 'H', 2 }, { 'V', 2 }, { 'W', 2 },
                        { 'Y', 2 }, { 'K', 1 }, { 'J', 1 }, { 'X', 1 },
                        { 'Q', 1 }, { 'Z', 1 }                       
                    };
                    break;
                case "pt-pt":
                case "pt-br":
                    values = new Dictionary<char, int>()
                    {
                        { ' ', 3 },
                        { 'A', 14 }, { 'E', 11}, { 'I', 10 }, { 'O', 10 },
                        { 'S', 8 }, { 'U', 7}, { 'M', 6 }, { 'R', 6 },
                        { 'T', 5 }, { 'D', 5}, { 'L', 5 }, { 'C', 4 },
                        { 'P', 4 }, { 'N', 4}, { 'B', 3 }, { 'Ç', 2 },
                        { 'F', 2 }, { 'G', 2}, { 'H', 2 }, { 'V', 2 },
                        { 'J', 2 }, { 'Q', 1}, { 'X', 1 }, { 'Z', 1 }
                    };
                    break;
                default:
                    throw new ArgumentOutOfRangeException("Invalid Language");

            }
            return values[c];
        }

    }
}
