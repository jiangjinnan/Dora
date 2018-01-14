using System;

namespace App
{
    public class SystomClock : ISystomClock
    { 

        [TraceInterceptor("Test", Order = 1)]
        [CacheReturnValue(Order = 2)]
        public DateTime GetCurrentTime(DateTimeKind dateTimeKind)
        {
            switch (dateTimeKind)
            {
                case DateTimeKind.Local:return DateTime.UtcNow.ToLocalTime();
                case DateTimeKind.Unspecified: return DateTime.Now;
                default: return DateTime.UtcNow;
            }
        }
    }
}
