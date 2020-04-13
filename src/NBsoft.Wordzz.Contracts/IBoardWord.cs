namespace NBsoft.Wordzz.Contracts
{
    public interface IBoardWord
    {
        IBoardLetter[] Letters { get; }
        IBoardTile[] Tiles { get; }
        string Owner { get; }
        int Score { get; }
    }
}
