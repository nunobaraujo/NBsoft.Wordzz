using NBsoft.Wordzz.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NBsoft.Wordzz.Core.Repositories
{
    public interface IGameRepository
    {
        Task<GameDataModel> Add(GameDataModel game);
        Task<GameDataModel> Get(string gameId);
        Task<GameDataModel> Update(GameDataModel game);
        Task<IEnumerable<GameDataModel>> GetActive();
        Task<IEnumerable<GameDataModel>> GetByUser(string userName);

        Task<int> AddMoves(string gameId, IEnumerable<GameMoveDataModel> moves);
        Task<IEnumerable<GameMoveDataModel>> GetMoves(string gameId);
        Task<IEnumerable<GameMoveDataModel>> GetMovesByUser(string userId);
    }
}
