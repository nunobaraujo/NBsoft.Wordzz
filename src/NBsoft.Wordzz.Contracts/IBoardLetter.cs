namespace NBsoft.Wordzz.Contracts
{
    public interface IBoardLetter
    {
        string Letter { get; }
        int Value { get; }
        string Owner { get; }
    }
}
