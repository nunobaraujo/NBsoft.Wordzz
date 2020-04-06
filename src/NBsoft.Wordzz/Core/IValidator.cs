namespace NBsoft.Wordzz.Core
{
    interface IValidator
    {
        public bool Validate(string apiKey, string sessionToken);
    }
}
