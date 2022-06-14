namespace App
{
public class Invoker
{
    [FoobarInterceptor("Interceptor1")]
    public virtual void Invoke1() => Console.WriteLine("Invoker.Invoke1()");

    [FoobarInterceptor("Interceptor2")]
    public virtual void Invoke2() => Console.WriteLine("Invoker.Invoke2()");
}
}