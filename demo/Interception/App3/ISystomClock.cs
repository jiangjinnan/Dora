using System;

namespace App
{
    public interface ISystomClock
    {
        DateTime GetCurrentTime(DateTimeKind dateTimeKind);
    }
}
