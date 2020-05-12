using System;

namespace Wordzz.Contracts.Requests
{
    [Serializable]
    public class ChallengeGameRequest: QueueGameRequest
    {
        public string Challenged { get; set; }
    }
}
