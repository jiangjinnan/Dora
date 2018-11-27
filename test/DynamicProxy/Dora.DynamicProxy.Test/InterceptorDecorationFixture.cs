using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Xunit;

namespace Dora.DynamicProxy.Test
{
    public class InterceptorDecorationFixture
    {
        [Fact]
        public void GetInterceptors()
        {
            var invokeMethod = typeof(Foobar).GetMethod("Invoke");
            var fooProperty = typeof(Foobar).GetProperty("Foo");

            InterceptorDelegate interceptor = next => null;
            var interceptors = new Dictionary<MethodInfo, InterceptorDelegate>
            {
                [invokeMethod] = interceptor,
                [fooProperty.GetMethod] = interceptor
            };

            var interception = new InterceptorDecoration(interceptors, typeof(Foobar).GetInterfaceMap(typeof(IFoobar)));
            Assert.False(interception.IsEmpty);
            Assert.True(interception.Contains(invokeMethod));
            Assert.True(interception.Contains(fooProperty.GetMethod));
            Assert.False(interception.Contains(fooProperty.SetMethod));

            Assert.True(interception.IsInterceptable(invokeMethod));
            Assert.True(interception.IsInterceptable(fooProperty.GetMethod));
            Assert.False(interception.IsInterceptable(fooProperty.SetMethod));

            Assert.Same(interceptor, interception.GetInterceptor(invokeMethod));
            Assert.Same(interceptor, interception.GetInterceptor(fooProperty.GetMethod));

            Assert.Same(typeof(Foobar).GetMethod("Invoke"), interception.GetTargetMethod(typeof(IFoobar).GetMethod("Invoke")));

        }

        private interface IFoobar
        {
            string Foo { get; set; }
            string Bar { get; set; }
            void Invoke();
        }

        private class Foobar : IFoobar
        {
            public string Bar { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public string Foo { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public void Invoke()
            {
                throw new NotImplementedException();
            }
        }
    }
}
