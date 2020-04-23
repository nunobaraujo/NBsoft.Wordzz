namespace NBsoft.Wordzz.Contracts
{
    public interface IPlayWord
    {        
        IPlayLetter[] Letters { get; }
        int Score { get; }
    }
}
