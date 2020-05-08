namespace NBsoft.Wordzz.Contracts
{
    public interface IBoard
    {
        int Id { get; }
        string Name { get; }
        int BoardRows { get; }
        int BoardColumns { get; }
        IBoardTile[] Tiles { get; }        
    }
}
