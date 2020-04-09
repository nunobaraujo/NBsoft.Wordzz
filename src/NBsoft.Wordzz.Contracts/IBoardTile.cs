namespace NBsoft.Wordzz.Contracts
{
    public interface IBoardTile
    {
        int X { get; }
        int Y { get; }
        BonusType Bonus { get; }
    }
}
