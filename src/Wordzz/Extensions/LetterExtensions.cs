using NBsoft.Wordzz.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NBsoft.Wordzz.Extensions
{
    public static class LetterExtensions
    {
        public static int LetterValue(this char c, string language)
        {
            Dictionary<char, int> values;
            switch (language)
            {
                case "en-us":
                case "en-en":
                    // English letter points
                    values = new Dictionary<char, int>()
                    {
                        // Zero Points
                        { ' ', 0 },

                        // One point
                        { 'A', 1 }, { 'E', 1 }, { 'I', 1 }, { 'O', 1 }, { 'U', 1 },
                        { 'L', 1 }, { 'N', 1 }, { 'S', 1 }, { 'T', 1 }, { 'R', 1 },

                        // Two points
                        { 'D', 2 },   { 'G', 2 }, 

                        // Three points
                        { 'B', 3 }, { 'C', 3 }, { 'M', 3 }, { 'P', 3 },

                        // Four points
                        { 'F', 4 }, { 'H', 4 }, { 'V', 4 }, { 'W', 4 }, { 'Y', 4 }, 

                        // Five points
                        { 'K', 5 }, 

                        // Eight points
                        { 'J', 10 }, { 'X', 8 },

                        // Ten points
                        { 'Q', 10 }, { 'Z', 10 }
                    };
                    break;
                case "pt-pt":
                case "pt-br":
                    // portuguese letter points
                    values = new Dictionary<char, int>()
                    {
                        // Zero Points
                        { ' ', 0 },

                         // One point
                        { 'A', 1 }, { 'E', 1 }, { 'I', 1 }, { 'O', 1 }, { 'U', 1 },
                        { 'S', 1 }, { 'M', 1 }, { 'R', 1 }, { 'T', 1 },

                        // Two points
                        { 'D', 2 }, { 'L', 2 }, { 'C', 2 }, { 'P', 2 }, 
                        
                        // Three points
                        { 'N', 3 }, { 'B', 3 }, { 'Ç', 3 },

                        // Four points
                        { 'F', 4 }, { 'G', 4 }, { 'H', 4 }, { 'V', 4 },

                        // Five points
                        { 'J', 5 }, 

                        // Six points
                        { 'Q', 6 },

                        // Eight  points
                        { 'X', 8 }, { 'Z', 8 }
                    };
                    break;
                default:
                    throw new ArgumentOutOfRangeException("Invalid Language");
            }
            return values[c];
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

        public static char[] GetLetters(this System.Globalization.CultureInfo language)
        {
            switch (language.Name.ToLower())
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
        
    }
}
