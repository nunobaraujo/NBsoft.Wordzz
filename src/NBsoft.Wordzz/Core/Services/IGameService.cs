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
        IGameQueue GetQueuedGame(string queueId);
        Task<IEnumerable<string>> GetContacts(string userId);

        Task<IGameChallenge> ChallengeGame(string language, string player1UserName, string player2UserName, int size);
        Task<IGame> AcceptChallenge(string challengedPlayer, string queueId, bool accept);
        IEnumerable<IGame> GetActiveGames(string userName);
        IEnumerable<string> GetActiveGameOpponents(string userName);
        IEnumerable<IGameChallenge> GetReceivedChallenges(string userName);
        IEnumerable<IGameChallenge> GetSentChallenges(string userName);

        Task<PlayResult> Play(string gameId, string username, PlayLetter[] letters);
        Task<PlayResult> Pass(string gameId, string username);

    }
}