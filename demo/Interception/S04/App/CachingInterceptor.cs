using Dora.Interception;
using Microsoft.Extensions.Caching.Memory;
using System.Reflection;

namespace App
{
    public class CachingInterceptor<TArgument, TReturnValue>
    {
        public async ValueTask InvokeAsync(InvocationContext invocationContext, IMemoryCache cache)
        {
            var key = new Tuple<MethodInfo, TArgument>(invocationContext.MethodInfo, invocationContext.GetArgument<TArgument>(0));
            if (cache.TryGetValue<TReturnValue>(key, out var value))
            {
                invocationContext.SetReturnValue(value);
                return;
            }

            await invocationContext.ProceedAsync();
            cache.Set(key, invocationContext.GetReturnValue<TReturnValue>());
        }
    }
}
