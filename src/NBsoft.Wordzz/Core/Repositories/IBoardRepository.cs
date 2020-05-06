using NBsoft.Wordzz.Contracts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NBsoft.Wordzz.Core.Repositories
{
    public interface IBoardRepository
    {
        Task<IBoard> Add(IBoard board);
        Task<IBoard> Get(int id);
        Task<IBoard> Update(IBoard board);
        Task<bool> Delete(int id);
        Task<IEnumerable<IBoard>> List();        
    }
}
