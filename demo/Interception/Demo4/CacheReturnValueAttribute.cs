using Dora.Interception;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Demo
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CacheReturnValueAttribute : InterceptorAttribute
    {
        public override void Use(IInterceptorChainBuilder builder)
        {
            builder.Use<CacheInterceptor>(this.Order);
        }
    }
}
