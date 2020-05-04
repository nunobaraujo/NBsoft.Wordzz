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
        Task<IEnumerable<GameDataModel>> GeByUser(string userName);

        Task<GameMoveDataModel> AddMove(GameMoveDataModel move);
        Task<IEnumerable<GameMoveDataModel>> GetMovesByPlayer();
    }
}
