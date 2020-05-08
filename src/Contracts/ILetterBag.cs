using System.Collections.Generic;

namespace NBsoft.Wordzz.Contracts
{
    public interface ILetterBag
    {
        string Language { get; }
        IEnumerable<ILetter> Bag { get; }
        IEnumerable<ILetter> TakeLetters(int letterCount);
    }
}
