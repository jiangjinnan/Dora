using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Dora.DynamicProxy.Test
{
    public class VirtualMethodDynamicProxyGeneratorFixture
    {
        private static int _flag = 0;
        [Fact]
        public void Create()
        {
            var foo1Method = typeof(Foobar).GetMethod("Foo1");
            var foo2Method = typeof(Foobar).GetMethod("Foo2");
            var bar1Property = typeof(Foobar).GetProperty("Bar1");
            var bar2Property = typeof(Foobar).GetProperty("Bar2");

            InterceptorDelegate interceptor = next => (context => Task.Run(() => _flag++));
            var interceptors = new Dictionary<MethodInfo, InterceptorDelegate>
            {
                [foo1Method] = interceptor,
                [foo2Method] = interceptor,
                [bar1Property.GetMethod] = interceptor ,
                [bar2Property.GetMethod] = interceptor,
            };
            var interception = new InterceptorDecoration(interceptors); 
            var proxy = new VirtualMethodDynamicProxyGenerator(new DynamicProxyFactoryCache())
                .Create(typeof(Foobar), interception,new ServiceCollection().BuildServiceProvider())
                as Foobar;

            _flag = 0;
            proxy.Foo1();
            Assert.Equal(1, _flag);
            proxy.Foo2();
            Assert.Equal(1, _flag);

            var bar = proxy.Bar1;
            Assert.Equal(2, _flag);
            bar = proxy.Bar2;
            Assert.Equal(2, _flag);

            proxy.Bar1 = "123";
            Assert.Equal(2, _flag);

            proxy.Bar2 = "123";
            Assert.Equal(2, _flag);

        }
        public class Foobar
        {
            public virtual void Foo1() { }

            public void Foo2() { }

            public virtual string Bar1 { get; set; }
            public  string Bar2 { get; set; }
        }
    }
}
