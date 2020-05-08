namespace NBsoft.Wordzz.Contracts
{
    public interface IBoardTile
    {
        int Id { get; }
        int BoardId { get; }
        int X { get; }
        int Y { get; }
        BonusType Bonus { get; }
    }
}
