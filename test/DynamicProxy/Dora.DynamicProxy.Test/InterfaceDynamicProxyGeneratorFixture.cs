using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Dora.DynamicProxy.Test
{
    public class InterfaceDynamicProxyGeneratorFixture
    {
        private static int _flag = 0;

        [Fact]
        public void Wrap()
        {
            var fooMethod = typeof(IFoobar).GetMethod("Foo");
            var barProperty = typeof(IFoobar).GetProperty("Bar");

            InterceptorDelegate interceptor = next => (context => Task.Run(() => _flag++)) ;
            var interceptors = new Dictionary<MethodInfo, InterceptorDelegate>
            {
                [fooMethod] = interceptor,
                [barProperty.GetMethod] = interceptor
            };
            var interception = new InterceptorDecoration(interceptors, typeof(Foobar).GetInterfaceMap(typeof(IFoobar)));

            var proxy = new InterfaceDynamicProxyGenerator(new DynamicProxyFactoryCache())
                .Wrap(typeof(IFoobar), new Foobar(), interception)
                as IFoobar;

            _flag = 0;
            proxy.Foo();
            Assert.Equal(1, _flag);
            var bar = proxy.Bar;
            Assert.Equal(2, _flag);
            proxy.Bar = "111";
            Assert.Equal(2, _flag);

        }

        public interface IFoobar
        {
            void Foo();
            string Bar { get; set; } 
        }

        public class Foobar : IFoobar
        {
            public string Bar { get; set; }

            public void Foo() { }
        }
    }
}
