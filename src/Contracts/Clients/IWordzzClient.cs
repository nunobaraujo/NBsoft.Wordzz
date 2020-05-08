using NBsoft.Wordzz.Contracts.Api;
using NBsoft.Wordzz.Contracts.Results;
using System;
using System.Threading.Tasks;

namespace NBsoft.Wordzz.Contracts.Clients
{
    public interface IWordzzClient:IDisposable
    {        
        ILexiconApi LexiconApi { get; }

        Task<LogInResult> Login(string userName, string password);
        Task Logout();

    }
}
