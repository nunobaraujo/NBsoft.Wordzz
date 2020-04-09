namespace NBsoft.Wordzz.Contracts.Clients
{
    public static class ClientFactory
    {
        public static IWordzzClient Create(string url)
        {
            return new WordzzClient(url);
        }

    }
}
