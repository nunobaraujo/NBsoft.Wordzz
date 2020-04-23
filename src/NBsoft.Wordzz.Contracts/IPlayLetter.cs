namespace NBsoft.Wordzz.Contracts
{
    public interface IPlayLetter
    {
        IBoardLetter Letter { get; }
        IBoardTile Tile { get; }
        BonusType EffectiveBonus { get; }
    }
}
