using NBsoft.Wordzz.Contracts;
using System.Collections.Generic;
using System.Linq;

namespace NBsoft.Wordzz.Cache
{
    internal sealed class DictionaryCache
    {
        private static readonly object padLock = new object();
        private static DictionaryCache instance = null;

        private readonly Dictionary<string, List<IWord>> cache;
        DictionaryCache()
        {
            cache = new Dictionary<string, List<IWord>>();
        }

        public static DictionaryCache Instance
        {
            get
            {
                lock (padLock)
                {
                    if (instance == null)
                        instance = new DictionaryCache();
                    return instance;
                }
            }
        }

        public void AddOrUpdateDictionary(string language, IEnumerable<IWord> dictionary)
        {
            var upperLanguage = language.ToUpper();
            lock (padLock)
            {
                if (cache.ContainsKey(upperLanguage))
                    cache.Remove(upperLanguage);

                cache.Add(upperLanguage, dictionary.ToList());
            }
        }
        public bool AddOrUpdateWord(IWord word)
        {
            var upperLanguage = word.Language.ToUpper();
            
            if (!cache.ContainsKey(upperLanguage))
                return false;

            lock (padLock)
            {
                var dictionary = cache[upperLanguage];
                var existing = dictionary.SingleOrDefault(w => w.Name.ToUpper() == word.Name.ToUpper());
                if (existing != null)
                    dictionary.Remove(existing);

                dictionary.Add(word);
            }
            return true;
        }

        public IWord GetWord(string language, string word) 
        {
            var upperLanguage = language.ToUpper();

            if (!cache.ContainsKey(upperLanguage))
                return null;

            return cache[upperLanguage].SingleOrDefault(x => x.Name.ToUpper() == word.ToUpper());
        }
    }
}
