﻿using Dora.Interception;

namespace App
{
    public interface ISystemTimeProvider
    {
        DateTime GetCurrentTime(DateTimeKind kind);
    }
    public class SystemTimeProvider : ISystemTimeProvider
    {
        [Interceptor(typeof(CachingInterceptor), Order = 1)]
        public virtual DateTime GetCurrentTime(DateTimeKind kind) => kind switch
        {
            DateTimeKind.Utc => DateTime.UtcNow,
            _ => DateTime.Now
        };
    }
}