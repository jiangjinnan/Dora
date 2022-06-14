using Dora.Interception;

//[assembly: NonInterceptable]

namespace App
{
[FoobarInterceptor]
public class Foo
{
    [NonInterceptable]
    public virtual void M() { }

    [NonInterceptable]
    public virtual object? P1 { get; set; }
    public virtual object? P2 { [NonInterceptable] get; set; }
}

[NonInterceptable]
public class Bar
{
    [FoobarInterceptor]
    public virtual void M() { }

    [FoobarInterceptor]
    public virtual object? P { get; set; }
}
}