using System;

namespace NBsoft.Wordzz.Contracts.Entities
{
    [Serializable]
    public class PlayLetter : IPlayLetter
    {
        public BoardLetter Letter { get; set; }
        public BoardTile Tile { get; set; }
        public BonusType EffectiveBonus { get; set; }

        IBoardLetter IPlayLetter.Letter => Letter;
        IBoardTile IPlayLetter.Tile => Tile;
    }
}
