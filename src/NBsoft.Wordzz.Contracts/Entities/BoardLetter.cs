using System;

namespace NBsoft.Wordzz.Contracts.Entities
{
    [Serializable]
    public class BoardLetter : IBoardLetter
    {
        public string Letter { get; set; }
        public int Value { get; set; }
        public string Owner { get; set; }
    }
}
