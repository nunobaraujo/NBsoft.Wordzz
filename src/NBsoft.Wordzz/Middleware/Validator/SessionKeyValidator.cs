using NBsoft.Wordzz.Core;
using NBsoft.Wordzz.Core.Services;

namespace NBsoft.Wordzz.Middleware.Validator
{
    class SessionKeyValidator : IValidator
    {
        private readonly string _apiKey;
        private readonly ILicenseService _licenseService;
        private readonly ISessionService _sessionService;

        public SessionKeyValidator(string apiKey, ILicenseService licenseService, ISessionService sessionService)
        {
            _apiKey = apiKey;
            _licenseService = licenseService;
            _sessionService = sessionService;
        }

        public bool Validate(string apiKey, string sessionToken)
        {
            return apiKey == _apiKey && _licenseService.IsLicensed && _sessionService.ValidateSession(sessionToken);
        }
    }
}
