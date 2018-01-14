using System;
using System.Collections.Generic;
using System.Text;

namespace App
{
    public class SystomClock : ISystomClock
    {
        [CacheReturnValue]
        public DateTime GetCurrentTime(DateTimeKind dateTimeKind)
        {
            switch (dateTimeKind)
            {
                case DateTimeKind.Local: return DateTime.UtcNow.ToLocalTime();
                case DateTimeKind.Unspecified: return DateTime.Now;
                default: return DateTime.UtcNow;
            }
        }
    }
}
