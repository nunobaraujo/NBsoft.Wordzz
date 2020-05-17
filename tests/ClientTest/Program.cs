using NBsoft.Wordzz.Contracts.Clients;
using NBsoft.Wordzz.Contracts.Requests;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ClientTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            Console.ReadLine();
            try
            {
                /*using (var client = ClientFactory.Create("http://localhost:5005"))
                {
                    
                    var user = await client.Login("sa", "#Na123@10");

                    var dictionary = await client.LexiconApi.GetDictionary("en-us");

                    var data = await dictionary.ReadAsStreamAsync();
                    SaveDictionary(data, (int)dictionary.Headers.ContentLength.Value, "en-us");
                    Console.WriteLine(dictionary);
                    
                    var dictionary = ReadDictionaryFile(@"D:\dev\en-us.txt");

                    var result = await client.LexiconApi.AddDictionary(new DictionaryRequest
                    {
                        Description = "US English Dictionary",
                        Language = "en-us",
                        Words = dictionary
                    });
                    

                }*/

                //ConvertCommasToJson(new CultureInfo("en-us"), @"D:\dev\en-us.txt");

                var rawWords = ConvertPtDic(@"D:\dev\wordlist-ao-latest_UTF8.txt");
                //var words = ExcludeProperNouns(rawWords)
                var words = rawWords.Select(w => RemoveSpecials(w))
                    .ToList();                
                words.Sort();
                WriteCommas(new CultureInfo("pt-pt"), words);
                WriteJson(new CultureInfo("pt-pt"), words);



            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadKey();
            }            
        }

        private static IEnumerable<string> ExcludeProperNouns(IEnumerable<string> words)
        {
            var result = new List<string>();
            string propertNounsFile = @"D:\dev\nomes.txt";
            var nouns = ReadCSV(propertNounsFile).Distinct().Select(n => n.ToUpper());

            foreach (var word in words.Select(w => w.ToUpper()))
            {
                if (!nouns.Contains(word))
                {
                    result.Add(word);
                }
                else
                    Console.WriteLine($"Excluded: {word}");
            }
            return result;

        }

        private static void ConvertCommasToJson(CultureInfo culture, string originFile)
        {
            var words = ReadCommaDictionaryFile(originFile);
            WriteJson(culture, words);
        }
        

        private static IEnumerable<string> ConvertPtDic(string file)
        {
            var words = ReadDictionaryFile(file);
            var processedWords = new List<string>();
            foreach (var word in words)
            {
                var index = word.IndexOf('/');
                if (index == -1)
                    index = word.IndexOf('\t');
                if (index == -1)
                    index = word.IndexOf(' ');
                
                var pword = word.ToUpper();
                if (index > 0)
                    pword = word.Substring(0, index).ToUpper();

                if (pword.Contains('-'))
                    continue;
                                                
                processedWords.Add(pword);
            }            
            return processedWords;
        }
        public static string RemoveSpecials(string word)
        {
            var clean = word
                    .Replace("Á", "A")
                    .Replace("À", "A")
                    .Replace("Â", "A")
                    .Replace("Ã", "A")

                    .Replace("É", "E")
                    .Replace("È", "E")
                    .Replace("Ê", "E")

                    .Replace("Í", "I")
                    .Replace("Ì", "I")
                    .Replace("Î", "I")

                    .Replace("Ó", "O")
                    .Replace("Ò", "O")
                    .Replace("Ô", "O")
                    .Replace("Õ", "O")

                    .Replace("Ú", "U")
                    .Replace("Ù", "U")
                    .Replace("Û", "U");
            return clean;
        }

        private static IEnumerable<string> ReadCSV(string file)
        {            
            var words = new List<string>();
            using (var fi = new System.IO.FileStream(file, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            using (var sr = new System.IO.StreamReader(fi))
            {
                while (!sr.EndOfStream)
                {
                    var word = sr.ReadLine();
                    var comma = word.IndexOf(',');
                    if (comma > -1)
                        word = word.Substring(0, comma);
                    words.Add(word);
                }
            }
            return words;
        }
        private static IEnumerable<string> ReadCommaDictionaryFile(string file)
        {
            var result = new List<string>();
            string words="";
            using (var fi = new System.IO.FileStream(file, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            using (var sr = new System.IO.StreamReader(fi))
            {
                while (!sr.EndOfStream)
                {
                    words = sr.ReadToEnd();
                }
            }
            return words.Split(',');
        }
        private static List<string> ReadDictionaryFile(string file)
        {
            var result = new List<string>();
            using (var fi = new System.IO.FileStream(file, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            using (var sr = new System.IO.StreamReader(fi, Encoding.UTF8))
            {
                while (!sr.EndOfStream)
                {
                    var word = sr.ReadLine();
                    result.Add(word);
                }
            }
            return result;
        }
        private static string SaveDictionary(Stream webStream, string filename, bool compressed) 
        {
            string value = "";
            if (compressed)
                value = Zip.Decompress(ReadToEnd(webStream));
            else
                value = Encoding.UTF8.GetString(ReadToEnd(webStream));

            var fi = new FileInfo(filename);
            using var fsw = fi.OpenWrite();
            using var sw = new StreamWriter(fsw);
            sw.Write(value);
            return fi.FullName;
        }
        public static byte[] ReadToEnd(Stream stream)
        {
            long originalPosition = 0;

            if (stream.CanSeek)
            {
                originalPosition = stream.Position;
                stream.Position = 0;
            }

            try
            {
                byte[] readBuffer = new byte[4096];

                int totalBytesRead = 0;
                int bytesRead;

                while ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
                {
                    totalBytesRead += bytesRead;

                    if (totalBytesRead == readBuffer.Length)
                    {
                        int nextByte = stream.ReadByte();
                        if (nextByte != -1)
                        {
                            byte[] temp = new byte[readBuffer.Length * 2];
                            Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                            Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                            readBuffer = temp;
                            totalBytesRead++;
                        }
                    }
                }

                byte[] buffer = readBuffer;
                if (readBuffer.Length != totalBytesRead)
                {
                    buffer = new byte[totalBytesRead];
                    Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
                }
                return buffer;
            }
            finally
            {
                if (stream.CanSeek)
                {
                    stream.Position = originalPosition;
                }
            }
        }

        private static void WriteJson(CultureInfo culture, IEnumerable<string> words)
        {
            var lex = new Lexicon()
            {
                Language = culture.Name,
                Description = culture.DisplayName,
                Words = words
            };
            byte[] byteArray = Encoding.UTF8.GetBytes(lex.ToJson());
            MemoryStream stream = new MemoryStream(byteArray);

            var destination = $@"D:\dev\{lex.Language}.json";
            SaveDictionary(stream, destination, false);
        }
        private static void WriteCommas(CultureInfo culture, IEnumerable<string> words)
        {
            string value = string.Join(',', words);
            byte[] byteArray = Encoding.UTF8.GetBytes(value);
            MemoryStream stream = new MemoryStream(byteArray);

            var destination = $@"D:\dev\{culture.Name}.csv";
            SaveDictionary(stream, destination, false);
        }

        private class Lexicon
        {
            public string Description { get; set; }
            public string Language { get; set; }
            public IEnumerable<string> Words { get; set; }

        }
    }
}
