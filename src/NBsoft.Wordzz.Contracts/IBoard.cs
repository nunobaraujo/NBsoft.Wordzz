namespace NBsoft.Wordzz.Contracts
{
    public interface IBoard
    {
        int Rows { get; }
        int Columns { get; }
        IBoardTile[] Tiles { get; }        
    }
}
