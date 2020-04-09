using System;

namespace NBsoft.Wordzz.Contracts.Entities
{
    public class Lexicon : ILexicon
    {
        public string Language { get; set; }
        public DateTime CreationDate { get; set; }
        public string Description { get; set; }
    }
}
