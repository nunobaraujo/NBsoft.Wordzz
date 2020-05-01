using NBsoft.Wordzz.Contracts;
using System.Threading.Tasks;

namespace NBsoft.Wordzz.Core.Services
{
    public interface ILexiconService
    {
        Task<ILexicon> GetDictionary(string language);
        Task<bool> ValidateWord(string language, string word);
        Task<IWord> GetWordInfo(string language, string words);
        Task LoadDictionary(string language);
    }
}
