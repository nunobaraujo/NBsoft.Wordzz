using NBsoft.Wordzz.Contracts;

namespace NBsoft.Wordzz.Core.Services
{
    public interface IGameService
    {
        IBoard GenerateBoard(int rows, int columns);
    }
}