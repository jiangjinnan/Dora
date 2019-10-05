using System;
using System.Threading.Tasks;

namespace App
{
    public interface ISystemClock
    {
        Task<DateTime> GetCurrentTimeAsync(DateTimeKind dateTimeKind);
    }
}
