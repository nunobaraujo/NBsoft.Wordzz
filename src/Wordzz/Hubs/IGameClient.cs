using NBsoft.Wordzz.Contracts;
using NBsoft.Wordzz.Contracts.Results;
using System.Threading.Tasks;

namespace NBsoft.Wordzz.Hubs
{
    public interface IGameClient
    {
        Task Connected(string user);
        Task Disconnected(string user);

        Task BroadCastMessage(string sender, string message);
        Task NewChallenge(IGameChallenge challenge);
        Task ChallengeAccepted(string challengeId, bool accept, string gameId);
        Task ChallengeCanceled(string challengeId);

        Task PlayOk(string gameId, string username);
        Task GameOver(string gameId, GameResult result);
    }
}
