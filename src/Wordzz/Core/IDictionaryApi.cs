using System.Threading.Tasks;

namespace NBsoft.Wordzz.Core
{
    public interface IDictionaryApi
    {
        Task<string> GetDescription(string word);
    }
}
