using NBsoft.Wordzz.Contracts;
using NBsoft.Wordzz.Contracts.Entities;
using NBsoft.Wordzz.GameLogic;
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

        public LetterBag(string language, IEnumerable<ILetter> initialBag = null)
        {
            Language = language;
            culture = new CultureInfo(language);
            if (initialBag == null)
                this.bag = GenereateLetterBag(culture);
            else
                this.bag = initialBag.ToList();
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
