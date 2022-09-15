using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App1
{
    internal class Invoker
    {
        private readonly ActivitySource _source;
        private volatile int _counter = 0;
        private readonly HttpClient _httpClient = new();

        public Invoker(ActivitySource source, IHttpClientFactory httpClientFactory)
        {
            _source = source;
            _httpClient = httpClientFactory.CreateClient();
        }

        public async Task InvokeAsync()
        {
            var counter = _counter;
            using (var a = _source.StartActivity($"[{counter + 2}]Job"))
            {
                try
                {
                    await Task.Delay(100);
                    using (_source.StartActivity($"[{counter + 1}]foo"))
                    {
                        await _httpClient.GetStringAsync("http://localhost:5002/weatherforecast");
                        using (_source.StartActivity($"[{counter}]bar"))
                        {
                            await Task.Delay(1000);
                        }
                    }
                }
                catch (Exception ex)
                {
                    a?.RecordException(ex);
                }
            }
            _counter += 3;
        }
    }
}
