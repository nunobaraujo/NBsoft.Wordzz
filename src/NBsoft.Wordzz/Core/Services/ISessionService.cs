using NBsoft.Wordzz.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NBsoft.Wordzz.Core.Services
{
    interface ISessionService
    {
        Task<string> LogIn(string userName, string password, string userInfo);
        Task LogOut(string sessionToken);
        Task<IUserSession> GetSession(string sessionToken);
        Task<IEnumerable<IUserSession>> GetAll();
        bool ValidateSession(string sessionToken);

    }
}
