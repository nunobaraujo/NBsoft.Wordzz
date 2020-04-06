using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NBsoft.Wordzz.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace NBsoft.Wordzz.Middleware
{
    class SessionKeyAuthHandler : AuthenticationHandler<SessionKeyAuthOptions>
    {
        private readonly IValidator _sessionValidator;

        public SessionKeyAuthHandler(IOptionsMonitor<SessionKeyAuthOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, IValidator sessionValidator)
            : base(options, logger, encoder, clock)
        {
            _sessionValidator = sessionValidator;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Context.Request.Headers.TryGetValue(SessionKeyAuthOptions.ApiKeyHeaderName, out var apiKeyValue))
            {
                return AuthenticateResult.Fail("No api key header.");
            }
            if (!Context.Request.Headers.TryGetValue(SessionKeyAuthOptions.TokenHeaderName, out var tokenValue))
            {
                return AuthenticateResult.Fail("No token header.");
            }

            var apiKey = apiKeyValue.First();
            var sessionToken = tokenValue.First();
            if (!_sessionValidator.Validate(apiKey,tokenValue))
            {
                return AuthenticateResult.Fail("Invalid API key or session token.");
            }


            var identity = new ClaimsIdentity("apikey");
            var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), null, "apikey");
            return await Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
