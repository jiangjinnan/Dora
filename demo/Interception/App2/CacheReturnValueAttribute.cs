using Dora.Interception;
using System;

namespace App
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CacheReturnValueAttribute : InterceptorAttribute
    {
        public override void Use(IInterceptorChainBuilder builder)
        {
            builder.Use<CacheInterceptor>(Order);
        }   
    }
}
