using NBsoft.Wordzz.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NBsoft.Wordzz.Core.Repositories
{
    public interface ISessionRepository
    {        
        Task<ISession> Get(string token);
        Task<IEnumerable<ISession>> List();
        Task New(ISession session);
        Task Update(ISession session);
        Task Remove(string token);
    }
}
