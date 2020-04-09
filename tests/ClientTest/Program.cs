using NBsoft.Wordzz.Contracts.Clients;
using NBsoft.Wordzz.Contracts.Requests;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ClientTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            Console.ReadLine();
            try
            {
                using (var client = ClientFactory.Create("http://localhost:5005"))
                {
                    var user = await client.Login("sa", "#Na123@10");

                    var dictionary = await client.LexiconApi.GetDictionary("en-us");

                    var data = await dictionary.ReadAsStreamAsync();
                    SaveDictionary(data, (int)dictionary.Headers.ContentLength.Value, "en-us");
                    Console.WriteLine(dictionary);
                    /*
                    var dictionary = ReadDictionaryFile(@"D:\dev\en-us.txt");

                    var result = await client.LexiconApi.AddDictionary(new DictionaryRequest
                    {
                        Description = "US English Dictionary",
                        Language = "en-us",
                        Words = dictionary
                    });
                    */

                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadKey();
            }            
        }

        private static List<string> ReadDictionaryFile(string file)
        {
            var result = new List<string>();
            using (var fi = new System.IO.FileStream(file, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            using (var sr = new System.IO.StreamReader(fi))
            {
                while (!sr.EndOfStream)
                {
                    var word = sr.ReadLine();
                    result.Add(word);
                }
            }
            return result;
        }
        private static string SaveDictionary(Stream webStream, int size, string filename) 
        {            
            var decompressesd = Zip.Decompress(ReadToEnd(webStream));

            var fi = new FileInfo($@"D:\dev\{filename}.txt");
            using var fsw = fi.OpenWrite();
            using var sw = new StreamWriter(fsw);
            sw.Write(decompressesd);
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
    }
}
