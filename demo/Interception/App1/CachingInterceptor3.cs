using Dora.Interception;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace App1
{
    public class CachingInterceptor3
    {
        public async ValueTask InvokeAsync(InvocationContext invocationContext, IMemoryCache cache)
        {
            var method = invocationContext.MethodInfo;
            var arguments = Enumerable.Range(0, method.GetParameters().Length).Select(index => invocationContext.GetArgument<object>(index));
            var key = new Key(method, arguments);

            if (cache.TryGetValue<object>(key, out var value))
            {
                invocationContext.SetReturnValue(value);
                return;
            }
            await invocationContext.ProceedAsync();
            cache.Set(key, invocationContext.GetReturnValue<object>());
        }
    }
}
