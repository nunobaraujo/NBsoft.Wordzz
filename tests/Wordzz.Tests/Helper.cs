using NBsoft.Wordzz.Contracts;
using NBsoft.Wordzz.Contracts.Entities;
using NBsoft.Wordzz.Extensions;
using System.Collections.Generic;

namespace Wordzz.Tests
{
    static class Helper
    {
        public const string Language = "en-US";
        public static PlayMove PlayTotem()
        {
            return new PlayMove // TOTEM - 6 points (M double lettere)
            {
                Player = "p1",
                Score = 6,
                Letters = new List<IPlayLetter>{
                        new PlayLetter
                        {
                            Letter = new BoardLetter{ Letter = new Letter{ Char = 'T', Value = 'T'.LetterValue(Language) }, Owner = "p1" },
                            Tile = new BoardTile{ Bonus = BonusType.Center, X = 8, Y = 8 }
                        },
                        new PlayLetter
                        {
                            Letter = new BoardLetter{ Letter = new Letter{ Char = 'O', Value = 'O'.LetterValue(Language) }, Owner = "p1" },
                            Tile = new BoardTile{ Bonus = BonusType.Regular, X = 9, Y = 8 }
                        },
                        new PlayLetter
                        {
                            Letter = new BoardLetter{ Letter = new Letter{ Char = 'T', Value = 'T'.LetterValue(Language) }, Owner = "p1" },
                            Tile = new BoardTile{ Bonus = BonusType.Regular, X = 10, Y = 8 }
                        },
                        new PlayLetter
                        {
                            Letter = new BoardLetter{ Letter = new Letter{ Char = 'E', Value = 'E'.LetterValue(Language) }, Owner = "p1" },
                            Tile = new BoardTile{ Bonus = BonusType.Regular, X = 11, Y = 8 }
                        },
                        new PlayLetter
                        {
                            Letter = new BoardLetter{ Letter = new Letter{ Char = 'M', Value = 'M'.LetterValue(Language) }, Owner = "p1" },
                            Tile = new BoardTile{ Bonus = BonusType.DoubleLetter, X = 12, Y = 8 }
                        }
                    }.ToArray()
            };
        }
        public static PlayMove PlayHate()
        {
            return new PlayMove // HATE - 14 points (H Double Word)
            {
                Player = "p1",
                Score = 14,
                Letters = new List<IPlayLetter>{
                        new PlayLetter
                        {
                            Letter = new BoardLetter{ Letter = new Letter{ Char = 'H', Value = 'H'.LetterValue(Language) }, Owner = "p2" },
                            Tile = new BoardTile{ Bonus = BonusType.DoubleWord, X = 11, Y = 5 }
                        },
                        new PlayLetter
                        {
                            Letter = new BoardLetter{ Letter = new Letter{ Char = 'A', Value = 'A'.LetterValue(Language) }, Owner = "p2" },
                            Tile = new BoardTile{ Bonus = BonusType.Regular, X = 11, Y = 6 }
                        },
                        new PlayLetter
                        {
                            Letter = new BoardLetter{ Letter = new Letter{ Char = 'T', Value = 'T'.LetterValue(Language) }, Owner = "p1" },
                            Tile = new BoardTile{ Bonus = BonusType.Regular, X = 11, Y = 7 }
                        },
                    }.ToArray()
            };
        }
        public static List<PlayLetter> PlayRid()
        {
            return new List<PlayLetter>
            {
                new PlayLetter
                {
                    Letter = new BoardLetter{ Letter = new Letter{ Char = 'R', Value = 'R'.LetterValue(Helper.Language) }, Owner = "p1" },
                    Tile = new BoardTile{ Bonus = BonusType.DoubleLetter, X = 9, Y = 9 }
                },
                new PlayLetter
                {
                    Letter = new BoardLetter{ Letter = new Letter{ Char = 'I', Value = 'I'.LetterValue(Helper.Language) }, Owner = "p1" },
                    Tile = new BoardTile{ Bonus = BonusType.Regular, X = 10, Y = 9 }
                },
                new PlayLetter
                {
                    Letter = new BoardLetter{ Letter = new Letter{ Char = 'D', Value = 'D'.LetterValue(Helper.Language) }, Owner = "p1" },
                    Tile = new BoardTile{ Bonus = BonusType.Regular, X = 11, Y = 9 }
                }
            };
        }
        public static List<PlayLetter> PlayRit()
        {
            return new List<PlayLetter>
            {
                new PlayLetter
                {
                    Letter = new BoardLetter{ Letter = new Letter{ Char = 'R', Value = 'R'.LetterValue(Helper.Language) }, Owner = "p1" },
                    Tile = new BoardTile{ Bonus = BonusType.DoubleLetter, X = 9, Y = 9 }
                },
                new PlayLetter
                {
                    Letter = new BoardLetter{ Letter = new Letter{ Char = 'I', Value = 'I'.LetterValue(Helper.Language) }, Owner = "p1" },
                    Tile = new BoardTile{ Bonus = BonusType.Regular, X = 10, Y = 9 }
                },
                new PlayLetter
                {
                    Letter = new BoardLetter{ Letter = new Letter{ Char = 'T', Value = 'T'.LetterValue(Helper.Language) }, Owner = "p1" },
                    Tile = new BoardTile{ Bonus = BonusType.Regular, X = 11, Y = 9 }
                }
            };
        }
    }
}
