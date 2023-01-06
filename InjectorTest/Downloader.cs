using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace InjectorTest
{
    public static class Downloader
    {
        private static readonly HttpClient _client = new HttpClient();
        
        public static async Task DownloadFile(string url, string path)
        {
            using (var s = await _client.GetStreamAsync(new Uri(url)))
            {
                using (var fs = new FileStream(path, FileMode.CreateNew))
                {
                    await s.CopyToAsync(fs);
                }
            }
            
            Console.WriteLine("Download finished");
        }
    }
}