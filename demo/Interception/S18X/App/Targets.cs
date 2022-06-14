namespace App
{    
public class Foo
{
    [FoobarInterceptor]
    public void M() { }
}

public class Bar
{
    [FoobarInterceptor]
    public object? P { get; set; }
}

[FoobarInterceptor]
public class Baz
{
    public void M() { }
}
}