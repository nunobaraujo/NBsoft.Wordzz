using System;

namespace NBsoft.Wordzz.Contracts.Entities
{
    [Serializable]
    public class BoardWord : IBoardWord
    {
        public IBoardLetter[] Letters { get; set; }
        public IBoardTile[] Tiles { get; set; }
        public string Owner { get; set; }

        public int Score => throw new NotImplementedException();
    }
}
