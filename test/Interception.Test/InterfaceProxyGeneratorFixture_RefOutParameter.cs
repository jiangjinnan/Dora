using Dora.Interception;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Xunit;

namespace Interception.Test
{
    public class InterfaceProxyGeneratorFixture_RefOutParameter
    {
        private readonly IServiceProvider _serviceProvider = new ServiceCollection()
            .AddSingleton<Bar>()
            .BuildServiceProvider();

        [Fact]
        public void Intercept()
        {
            FakeInterceptor.Reset();
            Foobar.Reset();
            var generator = new InterfaceProxyGenerator(new FakeInterceptorRegistrationProvider());
            var proxyType = generator.Generate(typeof(IFoobar), typeof(Foobar));
            var proxy = (IFoobar)Activator.CreateInstance(proxyType, new object[] { _serviceProvider, new FakeInterceptorProvider(true, null) });
            int x = 0, y = 0;

            proxy.Compare1(ref x, ref y, out var result);

            Assert.Equal(1, FakeInterceptor.InvocationContext.Arguments[0]);
            Assert.Equal(2, FakeInterceptor.InvocationContext.Arguments[1]);
            Assert.IsType<Foobar>(FakeInterceptor.InvocationContext.Target);
            Assert.Equal(typeof(Foobar).GetMethod(nameof(Foobar.Compare1)), FakeInterceptor.InvocationContext.Method);
            Assert.True(FakeInterceptor.Invoked);
            Assert.True(Foobar.Invoked);
        }

        public interface IFoobar
        {
            void Compare1(ref int x, ref int y, out int result);
            void Compare2<T>(ref T x, ref T y, out int result) where T : IComparable<T>;
        }
        public class Bar { }
        public class Foobar : IFoobar
        {
            public Foobar(Bar bar)
            { }

            public static bool Invoked { get; private set; }
            public static void Reset() => Invoked = false;

            public void Compare1(ref int x, ref int y, out int result)
            {
                Invoked = true;
                result = x.CompareTo(y);
            }

            public void Compare2<T>(ref T x, ref T y, out int result) where T : IComparable<T>
            {
                Invoked = true;
                result = x.CompareTo(y);
            }
        }

        private class FakeInterceptorRegistrationProvider : IInterceptorRegistrationProvider
        {
            private readonly Func<IServiceProvider, object> _factory;
            public FakeInterceptorRegistrationProvider()
            {
                _factory = serviceProvider => ActivatorUtilities.CreateInstance<FakeInterceptor>(serviceProvider);
            }
            public IEnumerable<InterceptorRegistration> GetRegistrations(Type targetType)
            {
                yield return new InterceptorRegistration(_factory, typeof(Foobar).GetMethod(nameof(Foobar.Compare1)), 1);
                yield return new InterceptorRegistration(_factory, typeof(Foobar).GetMethod(nameof(Foobar.Compare1)), 1);
            }
        }

        private class FakeInterceptorProvider : IInterceptorProvider
        {
            public bool CaptureArguments { get; }
            public object ReturnValue { get; }
            public FakeInterceptorProvider(bool captureArguments, object returnValue)
            {
                CaptureArguments = captureArguments;
                ReturnValue = returnValue;
            }

            public IInterceptor GetInterceptor(MethodInfo method)
            {
                return new FakeInterceptor(CaptureArguments, ReturnValue);
            }
        }
    }
}
