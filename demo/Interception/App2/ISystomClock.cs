using System;

namespace App
{
public interface ISystemClock
{
    DateTime GetCurrentTime(DateTimeKind dateTimeKind);
}
}
