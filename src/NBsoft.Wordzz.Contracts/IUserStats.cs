namespace NBsoft.Wordzz.Contracts
{
    public interface IUserStats
    {
        string UserName { get; }
        uint GamesPlayed { get; }
        uint Victories { get; }
        uint Defeats { get; }
        uint Draws { get; }
        uint TotalScore { get; }
        uint HighScoreGame { get; }
        string HighScoreGameOpponent { get; }
        uint HighScorePlay { get; }
        string HighScorePlayOpponent { get; }
        uint HighScoreWord { get; }
        string HighScoreWordOpponent { get; }
        uint LowScoreGame { get; }
        string LowScoreGameOpponent { get; }
        uint LowScorePlay { get; }
        string LowScorePlayOpponent { get; }
        uint LowScoreWord { get; }
        string LowScoreWordOpponent { get; }
        string MostUsedWord { get; }
        string MostFrequentOpponent { get; }
    }
}
