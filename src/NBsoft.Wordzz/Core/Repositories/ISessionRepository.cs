using NBsoft.Wordzz.Core.Models;
using System.Threading.Tasks;

namespace NBsoft.Wordzz.Core.Repositories
{
    interface ISessionRepository
    {
        Task<IUserSession> Get(string token);
        Task New(IUserSession session);
        Task Update(IUserSession session);
        Task Remove(string token);
    }
}
