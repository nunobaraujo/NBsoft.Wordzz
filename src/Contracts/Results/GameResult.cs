using System;

namespace NBsoft.Wordzz.Contracts.Results
{
    [Serializable]
    public class GameResult
    {
        public string GameId { get; set; }
        public string Player1 { get; set; }
        public string Player2 { get; set; }
        public string Winner { get; set; }
        public int P1Score { get; set; }
        public double P1Average { get; set; }
        public int P1PlayCount { get; set; }
        public int P2Score { get; set; }
        public double P2Average { get; set; }
        public int P2PlayCount { get; set; }
        public double Duration { get; set; }
        public FinishReason Reason { get; set; }

    }
}
