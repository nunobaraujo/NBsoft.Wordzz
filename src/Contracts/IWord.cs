namespace NBsoft.Wordzz.Contracts
{
    public interface IWord
    {
        uint Id { get; }
        string Language { get; }
        string Name { get; }
        string Description { get; }
    }
}
