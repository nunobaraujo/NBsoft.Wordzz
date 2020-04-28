using NBsoft.Wordzz.Contracts;
using NBsoft.Wordzz.Contracts.Entities;
using NBsoft.Wordzz.Core.Models;
using NBsoft.Wordzz.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace NBsoft.Wordzz.Entities
{
    public class LetterBag : ILetterBag
    {
        private readonly CultureInfo culture;
        private readonly List<ILetter> bag;

        public IEnumerable<ILetter> Bag => bag;
        public string Language { get; }

        public LetterBag(string language)
        {
            Language = language;
            culture = new CultureInfo(language);
            bag = GenereateLetterBag(culture);
        }

        

        public IEnumerable<ILetter> TakeLetters(int letterCount)
        {
            var letters = new List<ILetter>();
            var random = new Random();

            while (letters.Count() < letterCount)
            {
                // Ran out of letters
                if (Bag.Count() == 0)
                    break;

                var randomLetter = bag[random.Next(0, bag.Count)];
                letters.Add(randomLetter);
                bag.Remove(randomLetter);
            }

            return letters;
        }


        private static List<ILetter> GenereateLetterBag(CultureInfo culture)
        {
            string language = culture.Name.ToLower();
            var charList = culture.GetLetters();
            var Letters = new List<char>();            
            foreach (char c in charList)
            {
                for (int x = 0; x < c.LetterCount(language); x++)
                {
                    Letters.Add(c);
                }
            }


            var result = new List<ILetter>();

            foreach (var letter in Letters)
            {
                result.Add(new Letter
                {
                    Char = letter,
                    Value = letter.LetterValue(language),
                    IsBlank = letter == ' '
                }); ;
            }
            return result;
        }

        
        
    }
}
