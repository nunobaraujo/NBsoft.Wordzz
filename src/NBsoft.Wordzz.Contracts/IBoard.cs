namespace NBsoft.Wordzz.Contracts
{
    public interface IBoard
    {
        string Name { get; }
        int Rows { get; }
        int Columns { get; }
        IBoardTile[] Tiles { get; }        
    }
}
