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
            switch (dateTimeKind)
            {
                case DateTimeKind.Local: return Task.FromResult( DateTime.UtcNow.ToLocalTime());
                case DateTimeKind.Unspecified: return Task.FromResult(DateTime.Now);
                default: return Task.FromResult(DateTime.UtcNow);
            }
        }
    }
}
