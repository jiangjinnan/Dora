using Dora.Interception;
using System.Diagnostics;

namespace App
{
public class FoobarInterceptorAttribute: InterceptorAttribute
{
    public string Name { get;  }
    public FoobarInterceptorAttribute(string name) => Name = name;
    public ValueTask InvokeAsync(InvocationContext invocationContext)
    {
        Console.WriteLine($"FoobarInterceptor '{Name}' is invoked.");
        return invocationContext.ProceedAsync();
    }
}

    public interface IFoobar { }
    public class Foobar : IFoobar { }
}
