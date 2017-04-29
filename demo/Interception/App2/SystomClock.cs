using System;
using System.Collections.Generic;
using System.Text;

namespace App
{
    public class SystomClock : ISystomClock
    {
        [CacheReturnValue]
        public DateTime GetCurrentTime()
        {
            return DateTime.UtcNow;
        }
    }
}
