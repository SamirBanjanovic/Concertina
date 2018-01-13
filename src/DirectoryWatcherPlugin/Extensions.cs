using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DirectoryWatcherPlugin
{
    public static class Extensions
    {
        public static async Task<bool> PostMessageAsync(this ReceivedFileInformation receivedFileInformation, string uri, string route)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(uri);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var content = new StringContent(JsonConvert.SerializeObject(receivedFileInformation));

                return await Task.FromResult<bool>(false);
            }
        }
    }
}
