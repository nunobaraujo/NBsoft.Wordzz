using Microsoft.AspNetCore.Authentication;

namespace NBsoft.Wordzz.Middleware
{
    public class SessionKeyAuthOptions : AuthenticationSchemeOptions
    {
        public const string ApiKeyHeaderName = "api-key";
        public const string TokenHeaderName = "session-token";
        public const string AuthenticationScheme = "Automatic";
    }
}
