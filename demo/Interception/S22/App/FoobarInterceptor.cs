using Dora.Interception;

namespace App
{
    public class FoobarInterceptor
    {
        public FoobarInterceptor(string name, IFoobar foobar)
        {
            Name = name;
            Foobar = foobar;
        }

        public string Name { get; }
        public IFoobar Foobar { get; }
        public ValueTask InvokeAsync(InvocationContext invocationContext)
        {
            Console.WriteLine($"{invocationContext.MethodInfo.Name} is intercepted by FoobarInterceptor {Name}.");
            Console.WriteLine($"Foobar is '{Foobar.GetType()}'.");
            return invocationContext.ProceedAsync();
        }
    }

    public interface IFoobar { }
    public class Foo : IFoobar { }
    public class Bar: IFoobar { }
}
