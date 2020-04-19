using System;

namespace NBsoft.Wordzz.Contracts.Entities
{
    [Serializable]
    public class WordPlay : IWordPlay
    {
        public DateTime PlayDate { get; set; }
        public IBoardLetter[] Letters { get; set; }
        public IBoardTile[] Tiles { get; set; }
        public string Owner { get; set; }
        public int Score { get; set; }
    }
}
