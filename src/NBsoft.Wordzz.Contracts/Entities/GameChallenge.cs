using System;

namespace NBsoft.Wordzz.Contracts.Entities
{
    [Serializable]
    public class GameChallenge : IGameChallenge
    {
        public string Id { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }
        public string Language { get; set; }
        public int Size { get; set; }
    }
}
