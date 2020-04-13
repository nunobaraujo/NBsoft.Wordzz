using NBsoft.Wordzz.Contracts;
using System.Threading.Tasks;

namespace NBsoft.Wordzz.Core.Services
{
    public interface IGameService
    {
        Task<IGame> NewGame(string player1UserName, string player2UserName, int rows, int columns);
        Task<IGame> NewGame(string player1UserName, int aiLevel, int rows, int columns);
        IBoard GenerateBoard(int rows, int columns);
    }
}