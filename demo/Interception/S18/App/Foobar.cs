namespace App
{
    [FoobarInterceptor]
    public class Foo
    {
        public virtual void M1() { }
        public void M2() { }
        public virtual object? P1 { get; set; }
        public object? P2 { get;   set; }
    }

    public class Bar
    {
        [FoobarInterceptor]
        public virtual object? P1 { get; set; }

        public virtual object? P2 { get; [FoobarInterceptor] set; }
    }
}