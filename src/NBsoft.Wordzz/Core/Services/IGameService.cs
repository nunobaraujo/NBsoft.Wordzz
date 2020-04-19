using NBsoft.Wordzz.Contracts;
using NBsoft.Wordzz.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NBsoft.Wordzz.Core.Services
{
    public interface IGameService
    {        
        IGameQueue GetQueuedGame(string queueId);
        Task<IEnumerable<string>> GetContacts(string userId);

        string ChallengeGame(string language, string player1UserName, string player2UserName, int size);
        Task<IGame> AcceptChallenge(string challengedPlayer, string queueId, bool accept);
        IEnumerable<IGame> GetActiveGames(string userName);


    }
}