using NBsoft.Wordzz.Contracts.Requests;
using Refit;
using System.Net.Http;
using System.Threading.Tasks;

namespace NBsoft.Wordzz.Contracts.Api
{
    public interface ILexiconApi
    {

        [Post("/Lexicon/Dictionary")]
        [Headers("Authorization: Bearer")]
        Task<ApiResponse<bool>> AddDictionary([Body]DictionaryRequest request);

        [Get("/Lexicon/Dictionary")]
        [Headers("Authorization: Bearer")]
        Task<HttpContent> GetDictionary([Query]string language);
    }
}
