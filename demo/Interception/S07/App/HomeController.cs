using Microsoft.AspNetCore.Mvc;

namespace App
{
    public class HomeController
    {
        [HttpGet("/local")]
        public string GetLocalTime([FromServices] ISystemTimeProvider provider) => $"{provider.GetCurrentTime(DateTimeKind.Local)}[{DateTime.Now}]";

        [HttpGet("/utc")]
        public string GetUtcTime([FromServices] ISystemTimeProvider provider) => $"{provider.GetCurrentTime(DateTimeKind.Utc)}[{DateTime.UtcNow}]";
    }
}
