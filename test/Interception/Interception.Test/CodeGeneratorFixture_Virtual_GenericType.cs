using Microsoft.Extensions.DependencyInjection;
using Xunit;
using static Dora.Interception.Test.Interceptor;

namespace Dora.Interception.Test
{
    [InterceptorReset]
    public class CodeGeneratorFixture_Virtual_GenericType
    {
        [Fact]
        public void M1()
        {
            var foobar = new ServiceCollection().AddSingleton<Foobar<string>>().BuildInterceptableServiceProvider().GetRequiredService<Foobar<string>>();
            _ = foobar.M1();
            Assert.True(Interceptor.EnsureInterceptorInvoked());
        }

        [Fact]
        public void M2()
        {
            var foobar = new ServiceCollection().AddSingleton<Foobar<string>>().BuildInterceptableServiceProvider().GetRequiredService<Foobar<string>>();
            _ = foobar.M2<string>(null!);
            Assert.True(Interceptor.EnsureInterceptorInvoked());
        }

        [Foo(Order = 1)]
        [Bar(Order = 2)]
        public class Foobar<T> 
        {
            public virtual T M1() => default!;
            public virtual T M2<T2>(T2 arg) => default!;
        }
    }
}
