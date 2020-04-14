using NBsoft.Wordzz.Contracts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NBsoft.Wordzz.Core.Services
{
    public interface IGameService
    {
        Task<IGame> NewGame(string language, string player1UserName, string player2UserName, int size);
        Task<IGame> NewGame(string language, string player1UserName, int aiLevel, int size);
        IBoard GenerateBoard(int size);

        Task<IEnumerable<string>> GetContacts(string userId);
    }
}