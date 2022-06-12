using Dora.Interception;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dora.Interception.Test
{
    public static class Interceptor
    {
        public static readonly List<object> Interceptors = new();
        public static void Reset() => Interceptors.Clear();

        public static bool EnsureInterceptorInvoked() => Interceptors[0] is FooAttribute && Interceptors[1] is BarAttribute;

        public class FooAttribute : InterceptorAttribute
        {
            public ValueTask InvokeAsync(InvocationContext invocationContext)
            {
                Interceptors.Add(this);
                return invocationContext.ProceedAsync();
            }
        }

        public class BarAttribute : InterceptorAttribute
        {
            public ValueTask InvokeAsync(InvocationContext invocationContext)
            {
                Interceptors.Add(this);
                return invocationContext.ProceedAsync();
            }
        }
    }
}
