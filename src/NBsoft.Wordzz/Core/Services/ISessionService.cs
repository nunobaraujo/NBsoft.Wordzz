using NBsoft.Wordzz.Core.Models;
using NBsoft.Wordzz.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NBsoft.Wordzz.Core.Services
{
    public interface ISessionService
    {
        Task<LogInResult> LogIn(string userName, string password, string userInfo);
        Task LogOut(string sessionToken);
        Task<ISession> GetSession(string sessionToken);
        Task<IEnumerable<ISession>> GetAll();        

    }
}
