using NBsoft.Wordzz.Contracts;
using NBsoft.Wordzz.Contracts.Entities;
using NBsoft.Wordzz.Core.Services;
using NBsoft.Wordzz.Extensions;
using NBsoft.Wordzz.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NBsoft.Wordzz.GameLogic
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
                    var hAlreadyExists = horizontalWords.SingleOrDefault(w =>
                        w.GetStartY() == word.GetStartY() &&
                        w.GetEndY() == word.GetEndY() &&
                        w.GetStartX() == word.GetStartX() &&
                        w.GetEndX() == word.GetEndX());
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
                    var vAlreadyExists = verticalWords.SingleOrDefault(w => 
                        w.GetStartY() == word.GetStartY() && 
                        w.GetEndY() == word.GetEndY() && 
                        w.GetStartX() == word.GetStartX() && 
                        w.GetEndX() == word.GetEndX());
                    
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
                    Letters = word.Letters.Select(l => l.ToDto<PlayLetter>()),
                    RawScore = word.Letters.CalculateRawScore(game.Language),
                    Score = word.Letters.CalculateEffectiveScore(game.Language, playLetters)
                };                
                scoredWords.Add(scored);
            };
            return scoredWords;
        }
    }
}
