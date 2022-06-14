using Dora.Interception;

namespace App
{
    public class Invoker
    {
        [Interceptor(typeof(FoobarInterceptor), "Interceptor1")]
        public virtual void Invoke1() => Console.WriteLine("Invoker.Invoke1()");

        [Interceptor(typeof(FoobarInterceptor), "Interceptor2")]
        public virtual void Invoke2() => Console.WriteLine("Invoker.Invoke2()");
    }
}
