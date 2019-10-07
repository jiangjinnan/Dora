using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading.Tasks;

namespace App
{
    public class SystemClock : ISystemClock
    {
        public SystemClock() { }

        [CacheReturnValue]
        public Task<DateTime> GetCurrentTimeAsync(DateTimeKind dateTimeKind)
        {
            return dateTimeKind switch
            {
                DateTimeKind.Local => Task.FromResult(DateTime.UtcNow.ToLocalTime()),
                DateTimeKind.Unspecified => Task.FromResult(DateTime.Now),
                _ => Task.FromResult(DateTime.UtcNow),
            };
        }
    }
}
