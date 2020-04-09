namespace NBsoft.Wordzz.Contracts
{
    public interface IBoardTile
    {
        short X { get; }
        short Y { get; }
        BonusType Bonus { get; }
    }
}
