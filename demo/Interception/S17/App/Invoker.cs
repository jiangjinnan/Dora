namespace App
{
    public class Invoker
    {
        [FoobarInterceptor("Invoke1")]
        public virtual void Invoke1() => Console.WriteLine("Invoker.Invoke1()");

        [FoobarInterceptor("Invoke2")]
        public virtual void Invoke2() => Console.WriteLine("Invoker.Invoke2()");
    }
}