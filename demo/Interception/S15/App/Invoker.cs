using Dora.Interception;

namespace App
{
    public class Invoker
    {
        [Interceptor(typeof(FoobarInterceptor))]
        public virtual void Invoke() => Console.WriteLine("Invoker.Invoke()");
    }
}
