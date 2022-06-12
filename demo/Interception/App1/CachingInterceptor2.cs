using Dora.Interception;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace App1
{
public class CachingInterceptor2<TArgument, TReturnValue>
{
    private readonly IMemoryCache _cache;
    public CachingInterceptor2(IMemoryCache cache) => _cache = cache;

    public async ValueTask InvokeAsync(InvocationContext invocationContext)
    {
        var key = new Tuple<MethodInfo, TArgument>(invocationContext.MethodInfo, invocationContext.GetArgument<TArgument>(0));
        if (_cache.TryGetValue<TReturnValue>(key, out var value))
        {
            invocationContext.SetReturnValue(value);
            return;
        }

        await invocationContext.ProceedAsync();
        _cache.Set(key, invocationContext.GetReturnValue<TReturnValue>());
    }
}
}
