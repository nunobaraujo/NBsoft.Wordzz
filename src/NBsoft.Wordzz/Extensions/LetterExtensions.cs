using System;
using System.Collections.Generic;

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
       
    }
}
