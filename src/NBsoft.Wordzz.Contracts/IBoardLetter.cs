namespace NBsoft.Wordzz.Contracts
{
    public interface IBoardLetter
    {
        ILetter Letter { get; }
        string Owner { get; }
    }
}
