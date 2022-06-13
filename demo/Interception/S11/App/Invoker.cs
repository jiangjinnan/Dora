using Dora.Interception;

namespace App
{
    public class Invoker
    {
        [Interceptor(typeof(BarInterceptor), Order = 2)]
        [Interceptor(typeof(BazInterceptor), Order = 3)]
        [Interceptor(typeof(FooInterceptor), Order = 1)]
        public virtual void Invoke() => Console.WriteLine("Invoker.Invoke()");
    }
}
