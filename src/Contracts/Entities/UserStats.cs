namespace NBsoft.Wordzz.Contracts.Entities
{
    public class UserStats : IUserStats
    {
        public string UserName { get; set; }
        public uint GamesPlayed { get; set; }
        public uint Victories { get; set; }
        public uint Defeats { get; set; }
        public uint Draws { get; set; }
        public uint TotalScore { get; set; }
        public uint HighScoreGame { get; set; }
        public string HighScoreGameOpponent { get; set; }
        public uint HighScorePlay { get; set; }
        public string HighScorePlayOpponent { get; set; }
        public uint HighScoreWord { get; set; }
        public string HighScoreWordName { get; set; }
        public string HighScoreWordOpponent { get; set; }       
        public string MostUsedWord { get; set; }
        public string MostFrequentOpponent { get; set; }
        public uint Forfeits { get; set; }
    }
}
