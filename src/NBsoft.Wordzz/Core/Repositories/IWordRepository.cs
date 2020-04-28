using NBsoft.Wordzz.Contracts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NBsoft.Wordzz.Core.Repositories
{
    public interface IWordRepository
    {        
        Task<IWord> Add(IWord word);
        Task<IWord> Update(IWord word);
        Task<bool> Delete(IWord word);
        Task<IWord> Get(uint wordId);
        Task<IWord> Get(ILexicon lexicon, string word);        
        Task<IEnumerable<string>> ListWords(string language);
        Task<IEnumerable<IWord>> GetAllWords(string language);

        Task<ILexicon> GetDictionary(string language);
        Task<IEnumerable<ILexicon>> ListDictionaries();
        Task<bool> AddDictionary(ILexicon lexicon, IEnumerable<IWord> words);
        
    }
}
