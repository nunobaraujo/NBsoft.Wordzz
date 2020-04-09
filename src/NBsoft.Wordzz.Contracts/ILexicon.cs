using System;

namespace NBsoft.Wordzz.Contracts
{
    public interface ILexicon
    {
        string Language { get; }
        DateTime CreationDate { get; }
        string Description { get; }
    }
}
