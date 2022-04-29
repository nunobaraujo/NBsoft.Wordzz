using NBsoft.Wordzz.Core.Models;
using System.Threading.Tasks;

namespace NBsoft.Wordzz.Core.Services
{
    public interface IUserService
    {
        Task<IUser> AddUser(string userName, string userEmail, string password);
        Task<bool> RequestPasswordChange(string userEmail);
    }
}
