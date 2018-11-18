using Dora.Interception;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace App
{
    public class HomeController : Controller
    {
        private readonly ISystemClock _clock;
        public HomeController(IInterceptable<ISystemClock> clockAccessor)
        {
            _clock = clockAccessor.Proxy;
        }

        [HttpGet("/{kind?}")]
        public async Task Index(string kind="local")
        {
            DateTimeKind dateTimeKind = string.Compare(kind, "utc", true) == 0
                ? DateTimeKind.Utc
                : DateTimeKind.Local;

            this.Response.ContentType = "text/html";
            await this.Response.WriteAsync("<html><body><ul>");
            for (int i = 0; i < 2; i++)
            {
                await this.Response.WriteAsync($"<li>{_clock.GetCurrentTime(dateTimeKind)}</li>");
                await Task.Delay(1000);
            }  
            await this.Response.WriteAsync("</ul><body></html>");
        }
    }
}
