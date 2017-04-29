using Dora.Interception;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Demo
{
    public class HomeController : Controller
    {
        private readonly ITimeProvider _clock;
        public HomeController(ITimeProvider clock)
        {
            _clock = clock;
        }

        [HttpGet("/")]
        public async Task Index()
        {
            this.Response.ContentType = "text/html";
            await this.Response.WriteAsync("<html><body><ul>");
            for (int i = 0; i < 5; i++)
            {
                await this.Response.WriteAsync($"<li>{_clock.GetCurrentTime()}({DateTime.UtcNow})</li>");
                await Task.Delay(1000);
            }
            await this.Response.WriteAsync("</ul><body></html>");
        }
    }

    public interface ITimeProvider
    {
        DateTime GetCurrentTime();
    }
    public class TimeProvider: ITimeProvider
    {
        private ISystomClock _clock;
        public TimeProvider(ISystomClock clock)
        {
            _clock = clock;
        }
        public DateTime GetCurrentTime()
        {
            return _clock.GetCurrentTime();
        }
    }
    public interface ISystomClock: IDisposable
    {
        DateTime GetCurrentTime();
    }

    public class SystomClock : ISystomClock
    {
        public void Dispose()
        {
            
        }

        [CacheReturnValue]
        public DateTime GetCurrentTime()
        {
            return DateTime.UtcNow;
        }
    }
}
