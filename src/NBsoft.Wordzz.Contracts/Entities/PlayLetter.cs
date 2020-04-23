using System;

namespace NBsoft.Wordzz.Contracts.Entities
{
    [Serializable]
    class PlayLetter : IPlayLetter
    {
        public IBoardLetter Letter { get; set; }
        public IBoardTile Tile { get; set; }
        public BonusType EffectiveBonus { get; set; }
    }
}
