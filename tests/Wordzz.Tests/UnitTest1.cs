using NBsoft.Wordzz.Contracts;
using NBsoft.Wordzz.Contracts.Entities;
using NBsoft.Wordzz.Core.Services;
using NBsoft.Wordzz.Extensions;
using NBsoft.Wordzz.Helpers;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Wordzz.Tests
{
    public class Tests
    {
        private ILexiconService lexiconService;
        

        [SetUp]
        public void Setup()
        {
            lexiconService = new MockLexiconService();
        }

        [Test]
        public async Task TestMoreThan3Words()
        {
            var game = new Game
            {
                Player01 = new GamePlayer { UserName = "p1" },
                Player02 = new GamePlayer { UserName = "p2" },
                Board = BoardHelper.GenerateBoard(15,"Standard"),
                Language = Helper.Language
            };
            
            // Previous moves TOTEM, HATE
            game.PlayMoves = new List<PlayMove>
            {
                Helper.PlayTotem(),
                Helper.PlayHate()
            };

            // Played RID
            var letters = Helper.PlayRid();

            var res = await game.ValidateMove(letters.ToArray(), lexiconService);
            Assert.AreEqual("OK",res.Result);
            Assert.AreEqual(4, res.Words.Count());
            var words = new List<string>();
            foreach (var word in res.Words)
            {
                words.Add(word.GetString());
            }

            // Result should be RID, OR, IT, HATED
            Assert.Contains("RID", words);
            Assert.Contains("OR", words);
            Assert.Contains("TI", words);
            Assert.Contains("HATED", words);
        }

        [Test]
        public async Task TestMoreThan3WordsScore()
        {
            var game = new Game
            {
                Player01 = new GamePlayer { UserName = "p1" },
                Player02 = new GamePlayer { UserName = "p2" },
                Board = BoardHelper.GenerateBoard(15, "Standard"),
                Language = Helper.Language
            };

            // Previous moves TOTEM, HATE
            game.PlayMoves = new List<PlayMove>
            {
                Helper.PlayTotem(),
                Helper.PlayHate()
            };

            // Played RID
            var letters = Helper.PlayRid();

            var res = await game.ValidateMove(letters.ToArray(), lexiconService);
            var scored = game.ScoreMove(res.Words, letters).ToArray();
            Assert.AreEqual(4, res.Words.Count());

            // RID scores 5, 4 raw score
            Assert.AreEqual(5, scored[0].Score);
            Assert.AreEqual(4, scored[0].RawScore);
            // OR scores 3, 2 raw score
            Assert.AreEqual(3, scored[1].Score);
            Assert.AreEqual(2, scored[1].RawScore);
            // TI scores 2, raw score 2
            Assert.AreEqual(2, scored[2].Score);
            Assert.AreEqual(2, scored[2].RawScore);
            // HATED scores 9, raw score 9
            Assert.AreEqual(9, scored[3].Score);
            Assert.AreEqual(9, scored[3].RawScore);
        }

        [Test]
        public async Task TesInvalidWord()
        {
            var game = new Game
            {
                Player01 = new GamePlayer { UserName = "p1" },
                Player02 = new GamePlayer { UserName = "p2" },
                Board = BoardHelper.GenerateBoard(15, "Standard"),
                Language = Helper.Language
            };

            // Previous moves TOTEM, HATE
            game.PlayMoves = new List<PlayMove>
            {
                Helper.PlayTotem(),
                Helper.PlayHate()
            };

            // Played RIT not a word in dictionary
            var letters = Helper.PlayRit();

            var res = await game.ValidateMove(letters.ToArray(), lexiconService);
            Assert.AreEqual("Invalid words", res.Result);
        }

    }
}