using System;

namespace Wordzz.Contracts.Requests
{
    [Serializable]
    public class ChallengeAcceptRequest
    {
        public string ChallengeId { get; set; }
        public bool Accept { get; set; }
    }
}
