using NBsoft.Wordzz.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NBsoft.Wordzz.Core.Repositories
{
    interface IUserRepository
    {
        Task<IUser> Add(IUser user);
        Task<IUser> Update(IUser user);
        Task Delete(string userName);
        
        Task<IEnumerable<IUser>> List();
        Task<IUser> Get(string userName);
        Task<IUser> GetByEmail(string email);
        
        Task<IUser> Auth(string userName, string userPassword);
        Task SetPassword(string userId, string password);
        
    }
}
