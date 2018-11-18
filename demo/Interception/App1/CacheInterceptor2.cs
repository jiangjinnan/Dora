using Dora.DynamicProxy;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Reflection;
using System.Threading.Tasks;

namespace App
{
    public class CacheInterceptor
    {
        private readonly IMemoryCache _cache;
        private readonly MemoryCacheEntryOptions _options;
        public CacheInterceptor(IMemoryCache cache, IOptions<MemoryCacheEntryOptions> optionsAccessor)
        {
            _cache = cache;
            _options = optionsAccessor.Value;
        }

        public async Task InvokeAsync(InvocationContext context)
        {
            var key = new Cachekey(context.Method, context.Arguments);
            if (_cache.TryGetValue(key, out object value))
            {
                context.ReturnValue = value;
            }
            else
            {
                await context.ProceedAsync();
                _cache.Set(key, context.ReturnValue, _options);
            }
        }

        private class Cachekey
        {
            public MethodBase Method { get; }
            public object[] InputArguments { get; }

            public Cachekey(MethodBase method, object[] arguments)
            {
                this.Method = method;
                this.InputArguments = arguments;
            }

            public override bool Equals(object obj)
            {
                Cachekey another = obj as Cachekey;
                if (null == another)
                {
                    return false;
                }
                if (!this.Method.Equals(another.Method))
                {
                    return false;
                }
                for (int index = 0; index < this.InputArguments.Length; index++)
                {
                    var argument1 = this.InputArguments[index];
                    var argument2 = another.InputArguments[index];
                    if (argument1 == null && argument2 == null)
                    {
                        continue;
                    }

                    if (argument1 == null || argument2 == null)
                    {
                        return false;
                    }

                    if (!argument2.Equals(argument2))
                    {
                        return false;
                    }
                }
                return true;
            }

            public override int GetHashCode()
            {
                int hashCode = this.Method.GetHashCode();
                foreach (var argument in this.InputArguments)
                {
                    hashCode = hashCode ^ argument.GetHashCode();
                }
                return hashCode;
            }
        }
    }
}
