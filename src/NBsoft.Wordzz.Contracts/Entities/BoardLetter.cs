using System;

namespace NBsoft.Wordzz.Contracts.Entities
{
    [Serializable]
    public class BoardLetter : IBoardLetter
    {
        public ILetter Letter { get; set; }
        public string Owner { get; set; }
    }
}
