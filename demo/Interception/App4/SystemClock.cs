using System;

namespace App
{
    public class SystemClock : ISystemClock
    {
        public virtual DateTime GetCurrentTime(DateTimeKind dateTimeKind)
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
