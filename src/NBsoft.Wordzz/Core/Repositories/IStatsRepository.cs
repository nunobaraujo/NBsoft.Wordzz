using NBsoft.Wordzz.Contracts;
using System.Threading.Tasks;

namespace NBsoft.Wordzz.Core.Repositories
{
    public interface IStatsRepository
    {
        Task<IUserStats> Get(string userName);
        Task<IUserStats> Add(IUserStats userStats);
        Task<IUserStats> Update(IUserStats userStats);
        Task Delete(string userName);
    }
}
