namespace NBsoft.Wordzz.Contracts
{
    public interface IGameChallenge
    {
        string Id { get; }
        string Origin { get; }
        string Destination { get; }
        string Language { get; }
        int BoardId { get; }
    }
}
