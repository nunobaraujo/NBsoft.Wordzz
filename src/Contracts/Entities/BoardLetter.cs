using System;

namespace NBsoft.Wordzz.Contracts.Entities
{
    [Serializable]
    public class BoardLetter : IBoardLetter
    {
        public Letter Letter { get; set; }
        public string Owner { get; set; }

        ILetter IBoardLetter.Letter => Letter;
    }
}
