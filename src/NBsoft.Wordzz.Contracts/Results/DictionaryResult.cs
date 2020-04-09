using NBsoft.Wordzz.Contracts.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace NBsoft.Wordzz.Contracts.Results
{
    [Serializable]
    public class DictionaryResult
    {
        public Lexicon Dictionary { get; set; }
        public IEnumerable<Word> Words { get; set; }
    }
}
