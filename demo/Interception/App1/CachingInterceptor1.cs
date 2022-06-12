using Dora.Interception;
using Microsoft.Extensions.Caching.Memory;

namespace App1
{
public class CachingInterceptor1
{
    private readonly IMemoryCache _cache;
    public CachingInterceptor1(IMemoryCache cache) => _cache = cache;

    public async ValueTask InvokeAsync(InvocationContext invocationContext)
    {
        var method = invocationContext.MethodInfo;
        var arguments = Enumerable.Range(0, method.GetParameters().Length).Select(index => invocationContext.GetArgument<object>(index));
        var key = new Key(method, arguments);

        if (_cache.TryGetValue<object>(key, out var value))
        {
            invocationContext.SetReturnValue(value);
            return;
        }
        await invocationContext.ProceedAsync();
        _cache.Set(key, invocationContext.GetReturnValue<object>());
    }
}
}
