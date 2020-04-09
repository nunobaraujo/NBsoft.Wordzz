using System;
using System.Collections.Generic;

namespace NBsoft.Wordzz.Contracts.Requests
{
    [Serializable]
    public class DictionaryRequest
    {
        public string Language { get; set; }
        public string Description { get; set; }        
        public IEnumerable<string> Words { get; set; }
    }
}
