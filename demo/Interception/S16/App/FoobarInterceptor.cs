using Dora.Interception;
using System.Diagnostics;

namespace App
{
public class FoobarInterceptor
{
    public string Name { get;  }
    public FoobarInterceptor(string name, IFoobar foobar)
    {
        Name = name;
        Debug.Assert(foobar is not null);
    }
    public ValueTask InvokeAsync(InvocationContext invocationContext)
    {
        Console.WriteLine($"FoobarInterceptor '{Name}' is invoked.");
        return invocationContext.ProceedAsync();
    }
}

public interface IFoobar { }
public class Foobar : IFoobar { }
}
