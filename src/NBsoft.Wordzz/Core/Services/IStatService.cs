using NBsoft.Wordzz.Contracts;
using System.Threading.Tasks;

namespace NBsoft.Wordzz.Core.Services
{
    public interface IStatService
    {
        Task<IUserStats> GetStats(string userName);
        void UpdateStats(string gameId);
    }
}
