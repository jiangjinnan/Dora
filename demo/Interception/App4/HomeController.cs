using Dora.Interception;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace App
{
    public class HomeController : Controller
    {
        private readonly ISystemClock _clock;
        public HomeController(ISystemClock  clock)
        {
            _clock = clock;
            //Debug.Assert(typeof(SystemClock) != _clock.GetType());
        }

        [HttpGet("/{kind?}")]
        public async Task Index(string kind = "local")
        {
            DateTimeKind dateTimeKind = string.Compare(kind, "utc", true) == 0
                ? DateTimeKind.Utc
                : DateTimeKind.Local;

            Response.ContentType = "text/html";
            await Response.WriteAsync("<html><body><ul>");
            for (int i = 0; i < 2; i++)
            {
                await Response.WriteAsync($"<li>{_clock.GetCurrentTime(dateTimeKind)}</li>");
                await Task.Delay(1000);
            }
            await Response.WriteAsync("</ul><body></html>");
        }
    }
}
