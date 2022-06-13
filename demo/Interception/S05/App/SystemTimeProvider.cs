using Dora.Interception;

namespace App
{
    public interface ISystemTimeProvider
    {
        DateTime GetCurrentTime(DateTimeKind kind);
    }

    [NonInterceptable]
    public class SystemTimeProvider : ISystemTimeProvider
    {
        [Interceptor(typeof(CachingInterceptor<DateTimeKind, DateTime>), Order = 1)]
        public virtual DateTime GetCurrentTime(DateTimeKind kind) => kind switch
        {
            DateTimeKind.Utc => DateTime.UtcNow,
            _ => DateTime.Now
        };
    }
}