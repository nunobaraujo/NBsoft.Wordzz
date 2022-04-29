using NBsoft.Wordzz.Contracts;
using NBsoft.Wordzz.Contracts.Entities;
using NBsoft.Wordzz.DictionaryApi;

namespace NBsoft.Wordzz.Extensions
{
    public static class DictionaryApiFactoryExtensions
    {
        public static IWord UpdateDescription(this DictionaryApiFactory src, IWord word)
        {
            var newDescritiptionTask = src
                        .CreateDictionaryApi(new System.Globalization.CultureInfo(word.Language))
                        .GetDescription(word.Name);
            newDescritiptionTask.Wait();
            var newDescritiption = newDescritiptionTask.Result;

            return new Word
            {
                Id = word.Id,
                Language = word.Language,
                Description = newDescritiption,
                Name = word.Name
            };
        }
    }
}
