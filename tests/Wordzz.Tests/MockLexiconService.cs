using NBsoft.Wordzz.Contracts;
using NBsoft.Wordzz.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Wordzz.Tests
{
    class MockLexiconService : ILexiconService
    {
        public List<string> WordList = new List<string>
        {
            "TOTEM", "HATE", "RID", "OR", "TI","HATED"
        }; 

        public Task<IEnumerable<ILexicon>> AvailableLexicons()
        {
            throw new NotImplementedException();
        }

        public Task<ILexicon> GetDictionary(string language)
        {
            throw new NotImplementedException();
        }

        public Task<IWord> GetWordInfo(string language, string words)
        {
            throw new NotImplementedException();
        }

        public Task LoadDictionary(string language)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ValidateWord(string language, string word)
        {
            if (WordList.Contains(word))
                return Task.FromResult(true);
            return Task.FromResult(false);

        }
    }
}
