namespace NBsoft.Wordzz.Contracts.Entities
{
    public class Word : IWord
    {
        public uint Id { get; set; }
        public string Language { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
