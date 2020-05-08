using NBsoft.Wordzz.Contracts;
using NBsoft.Wordzz.Contracts.Entities;
using NBsoft.Wordzz.Contracts.Results;
using NBsoft.Wordzz.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NBsoft.Wordzz.Core.Services
{
    public interface IGameService
    {   
        Task<IGameQueue> SearchGame(string language, int boardId, string userName);
        Task<IGameChallenge> ChallengeGame(string language, int boardId, string challenger, string challenged);
        
        Task<IGame> AcceptChallenge(string queueId, bool accept);

        Task<IEnumerable<string>> GetContacts(string userId);
        IEnumerable<IGame> GetActiveGames(string userName);
        IEnumerable<string> GetActiveGameOpponents(string userName);
        
        Task<PlayResult> Play(string gameId, string username, PlayLetter[] letters);
        Task<PlayResult> Pass(string gameId, string username);
        Task<PlayResult> Forfeit(string gameId, string username);


        Task<string> GetGameMatch(string userName);
    }
}