using Dora.Interception;

namespace App2
{
    public interface ISystemTimeProvider
    {
        DateTime GetCurrentTime(DateTimeKind kind);
    }
    public class SystemTimeProvider : ISystemTimeProvider
    {
        [Interceptor(typeof(CachingInterceptor<DateTimeKind, DateTime>))]
        public virtual DateTime GetCurrentTime(DateTimeKind kind) => kind switch
        {
            DateTimeKind.Utc => DateTime.UtcNow,
            _ => DateTime.Now
        };
    }
}