namespace App
{
    public class Invoker
    {
        [Bar(Order = 2)]
        [Baz(Order = 3)]
        [Foo(Order = 1)]
        public virtual void Invoke() => Console.WriteLine("Invoker.Invoke()");
    }
}
