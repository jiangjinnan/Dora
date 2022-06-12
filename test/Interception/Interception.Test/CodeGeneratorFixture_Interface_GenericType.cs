using Microsoft.Extensions.DependencyInjection;
using Xunit;
using static Dora.Interception.Test.Interceptor;

namespace Dora.Interception.Test
{
    [InterceptorReset]
    public class CodeGeneratorFixture_Interface_GenericType
    {
        [Fact]
        public void GetValue()
        {
            var foobar = new ServiceCollection().AddSingleton<IFoobar<string>, Foobar<string>>().BuildInterceptableServiceProvider().GetRequiredService<IFoobar<string>>();
            _ = foobar.M1();
            Assert.True(Interceptor.EnsureInterceptorInvoked());
        }

        public interface IFoobar<T>
        { 
            T M1();
        }

        [Foo(Order = 1)]
        [Bar(Order = 2)]
        public class Foobar<T> : IFoobar<T>
        {
            public T M1() => default!;
        }
    }
}
