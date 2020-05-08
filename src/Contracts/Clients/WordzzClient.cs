using NBsoft.Wordzz.Contracts.Api;
using NBsoft.Wordzz.Contracts.Results;
//using Refit;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NBsoft.Wordzz.Contracts.Clients
{
    internal class WordzzClient : IWordzzClient
    {
        //private IAuthenticationApi _authenticationApi;
        private readonly string _url;

        private LogInResult userData = null;
        //private RefitSettings settings;

        public ILexiconApi LexiconApi { get; private set; }

        public WordzzClient(string url)
        {
            _url = url;
        }
        
        public Task<LogInResult> Login(string userName, string password)
        {
            throw new NotImplementedException();
           /* _authenticationApi = RestService.For<IAuthenticationApi>(_url);

            var result = await _authenticationApi.Login(new Requests.LogInRequest
            {
                UserName = userName,
                Password = password
            });

            var loginResult = result.Content as LogInResult;
            if (loginResult == null)
                throw new ApplicationException(result.Error?.Message);
                        
            userData = loginResult;
            settings = new RefitSettings()
            {
                AuthorizationHeaderValueGetter = () => Task.FromResult(loginResult.Token)
            };

            _authenticationApi = RestService.For<IAuthenticationApi>(_url,settings);
            LexiconApi = RestService.For<ILexiconApi>(_url, settings);

            return loginResult;*/
        }

        public Task Logout()
        {
            throw new NotImplementedException();
            /*userData = null;
            await _authenticationApi.Logout();*/
        }

        public void Dispose()
        {
            if (userData != null)
                Task.Run(async () => await Logout())
                    .Wait();
        }
    }
}
