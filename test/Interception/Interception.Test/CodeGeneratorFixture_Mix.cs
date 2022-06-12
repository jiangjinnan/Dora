using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static Dora.Interception.Test.Interceptor;

namespace Dora.Interception.Test
{
    [InterceptorReset]
    public class CodeGeneratorFixture_Mix
    {
        [Fact]
        public void M1()
        {
           var foobar = new ServiceCollection().AddSingleton<IFoobar, Foobar>().BuildInterceptableServiceProvider().GetRequiredService<IFoobar>();
            foobar.M1();
            Assert.True(Interceptor.Interceptors[0] is FooAttribute);
            Assert.True(Interceptor.Interceptors[1] is BarAttribute);
            Assert.True(Interceptor.Interceptors[2] is FooAttribute);
            Assert.True(Interceptor.Interceptors[3] is BarAttribute);
        }

        public interface IFoobar
        {
            void M1();
        }

        [Foo(Order = 1)]
        [Bar(Order = 1)]
        public class Foobar : IFoobar
        {
            public void M1() => M2();
            protected virtual void M2() { }
        }
    }
}
