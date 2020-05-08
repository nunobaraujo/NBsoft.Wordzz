using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace NBsoft.Wordzz.Extensions
{
    public static class HttpContextExtensions
    {
        public static async Task<string> GetToken(this HttpContext src)
        {
            var result = await src.AuthenticateAsync();
            return result.Properties.Items[".Token.access_token"];
        }
    }
}
