using Dora.Interception;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App
{
    public class HomeController : Controller
    {
        private readonly ISystomClock _clock;
        public HomeController(IInterceptable<ISystomClock> clockAccessor)
        {
            _clock = clockAccessor.Proxy;
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
}
