using Dora.Interception;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace Interception.Test
{
    public class InterfaceProxyGeneratorFixture_GenericMethod
    {
        private readonly IServiceProvider _serviceProvider = new ServiceCollection()
            .AddSingleton<Bar>()
            .BuildServiceProvider();

        [Fact]
        public void InvokeAsVoid_CaptureArguments()
        {
            FakeInterceptor.Reset();
            Foobar.Reset();
            var generator = new InterfaceProxyGenerator(new FakeInterceptorRegistrationProvider());
            var proxyType = generator.Generate(typeof(IFoobar), typeof(Foobar));
            var proxy = (IFoobar)Activator.CreateInstance(proxyType, new object[] { _serviceProvider, new FakeInterceptorProvider(true, null)});
            proxy.InvokeAsVoid(1, 2);

            Assert.Equal(1, FakeInterceptor.InvocationContext.Arguments[0]);
            Assert.Equal(2, FakeInterceptor.InvocationContext.Arguments[1]);
            Assert.IsType<Foobar>(FakeInterceptor.InvocationContext.Target);
            Assert.Equal(typeof(Foobar).GetMethod(nameof(Foobar.InvokeAsVoid)).MakeGenericMethod(typeof(int)), FakeInterceptor.InvocationContext.Method);
            Assert.True(FakeInterceptor.Invoked);
            Assert.True(Foobar.Invoked);
        }

        [Fact]
        public void InvokeAsVoid_Not_CaptureArguments()
        {
            FakeInterceptor.Reset();
            Foobar.Reset();
            var generator = new InterfaceProxyGenerator(new FakeInterceptorRegistrationProvider());
            var proxyType = generator.Generate(typeof(IFoobar), typeof(Foobar));
            var proxy = (IFoobar)Activator.CreateInstance(proxyType, new object[] { _serviceProvider, new FakeInterceptorProvider(false, null) });
            proxy.InvokeAsVoid(1, 2);

            Assert.Null(FakeInterceptor.InvocationContext.Arguments);
            Assert.IsType<Foobar>(FakeInterceptor.InvocationContext.Target);
            Assert.Equal(typeof(Foobar).GetMethod(nameof(Foobar.InvokeAsVoid)).MakeGenericMethod(typeof(int)), FakeInterceptor.InvocationContext.Method);
            Assert.True(FakeInterceptor.Invoked);
            Assert.True(Foobar.Invoked);
        }

        [Fact]
        public void InvokeAsResult_CaptureArguments()
        {
            FakeInterceptor.Reset();
            Foobar.Reset();
            var generator = new InterfaceProxyGenerator(new FakeInterceptorRegistrationProvider());
            var proxyType = generator.Generate(typeof(IFoobar), typeof(Foobar));
            var proxy = (IFoobar)Activator.CreateInstance(proxyType, new object[] { _serviceProvider, new FakeInterceptorProvider(true, null) });
            var result = proxy.InvokeAsResult(1, 2);

            Assert.Equal(1, FakeInterceptor.InvocationContext.Arguments[0]);
            Assert.Equal(2, FakeInterceptor.InvocationContext.Arguments[1]);
            Assert.IsType<Foobar>(FakeInterceptor.InvocationContext.Target);
            Assert.Equal(typeof(Foobar).GetMethod(nameof(Foobar.InvokeAsResult)).MakeGenericMethod(typeof(int)), FakeInterceptor.InvocationContext.Method);
            Assert.True(FakeInterceptor.Invoked);
            Assert.True(Foobar.Invoked);
            Assert.Equal(-1,result);
        }

        [Fact]
        public void InvokeAsResult_Not_CaptureArguments()
        {
            FakeInterceptor.Reset();
            Foobar.Reset();
            var generator = new InterfaceProxyGenerator(new FakeInterceptorRegistrationProvider());
            var proxyType = generator.Generate(typeof(IFoobar), typeof(Foobar));
            var proxy = (IFoobar)Activator.CreateInstance(proxyType, new object[] { _serviceProvider, new FakeInterceptorProvider(false, null) });
            var result = proxy.InvokeAsResult(1, 2);

            Assert.Null(FakeInterceptor.InvocationContext.Arguments);
            Assert.Equal(typeof(Foobar).GetMethod(nameof(Foobar.InvokeAsResult)).MakeGenericMethod(typeof(int)), FakeInterceptor.InvocationContext.Method);
            Assert.True(FakeInterceptor.Invoked);
            Assert.True(Foobar.Invoked);
            Assert.Equal(-1, result);
        }

        [Fact]
        public void InvokeAsResult_OverrideReturnValue()
        {
            FakeInterceptor.Reset();
            Foobar.Reset();
            var generator = new InterfaceProxyGenerator(new FakeInterceptorRegistrationProvider());
            var proxyType = generator.Generate(typeof(IFoobar), typeof(Foobar));
            var proxy = (IFoobar)Activator.CreateInstance(proxyType, new object[] { _serviceProvider, new FakeInterceptorProvider(true, 100) });
            var result = proxy.InvokeAsResult(1, 2);
            Assert.Equal(100, result);
        }

        [Fact]
        public async void InvokeAsTask_CaptureArguments()
        {
            FakeInterceptor.Reset();
            Foobar.Reset();
            var generator = new InterfaceProxyGenerator(new FakeInterceptorRegistrationProvider());
            var proxyType = generator.Generate(typeof(IFoobar), typeof(Foobar));
            var proxy = (IFoobar)Activator.CreateInstance(proxyType, new object[] { _serviceProvider, new FakeInterceptorProvider(true, null) });
            await proxy.InvokeAsTask(1, 2);

            Assert.Equal(1, FakeInterceptor.InvocationContext.Arguments[0]);
            Assert.Equal(2, FakeInterceptor.InvocationContext.Arguments[1]);
            Assert.IsType<Foobar>(FakeInterceptor.InvocationContext.Target);
            Assert.Equal(typeof(Foobar).GetMethod(nameof(Foobar.InvokeAsTask)).MakeGenericMethod(typeof(int)), FakeInterceptor.InvocationContext.Method);
            Assert.True(FakeInterceptor.Invoked);
            Assert.True(Foobar.Invoked);
        }

        [Fact]
        public async void InvokeAsTask_Not_CaptureArguments()
        {
            FakeInterceptor.Reset();
            Foobar.Reset();
            var generator = new InterfaceProxyGenerator(new FakeInterceptorRegistrationProvider());
            var proxyType = generator.Generate(typeof(IFoobar), typeof(Foobar));
            var proxy = (IFoobar)Activator.CreateInstance(proxyType, new object[] { _serviceProvider, new FakeInterceptorProvider(false, null) });
            await proxy.InvokeAsTask(1, 2);

            Assert.Null(FakeInterceptor.InvocationContext.Arguments);
            Assert.IsType<Foobar>(FakeInterceptor.InvocationContext.Target);
            Assert.Equal(typeof(Foobar).GetMethod(nameof(Foobar.InvokeAsTask)).MakeGenericMethod(typeof(int)), FakeInterceptor.InvocationContext.Method);
            Assert.True(FakeInterceptor.Invoked);
            Assert.True(Foobar.Invoked);
        }

        [Fact]
        public async void InvokeAsTaskOfResult_CaptureArguments()
        {
            FakeInterceptor.Reset();
            Foobar.Reset();
            var generator = new InterfaceProxyGenerator(new FakeInterceptorRegistrationProvider());
            var proxyType = generator.Generate(typeof(IFoobar), typeof(Foobar));
            var proxy = (IFoobar)Activator.CreateInstance(proxyType, new object[] { _serviceProvider, new FakeInterceptorProvider(true, null) });
            var result = await proxy.InvokeAsTaskOfResult(1, 2);

            Assert.Equal(1, FakeInterceptor.InvocationContext.Arguments[0]);
            Assert.Equal(2, FakeInterceptor.InvocationContext.Arguments[1]);
            Assert.IsType<Foobar>(FakeInterceptor.InvocationContext.Target);
            Assert.Equal(typeof(Foobar).GetMethod(nameof(Foobar.InvokeAsTaskOfResult)).MakeGenericMethod(typeof(int)), FakeInterceptor.InvocationContext.Method);
            Assert.True(FakeInterceptor.Invoked);
            Assert.True(Foobar.Invoked);
            Assert.Equal(-1, result);
        }

        [Fact]
        public async void InvokeAsTaskOfResult_Not_CaptureArguments()
        {
            FakeInterceptor.Reset();
            Foobar.Reset();
            var generator = new InterfaceProxyGenerator(new FakeInterceptorRegistrationProvider());
            var proxyType = generator.Generate(typeof(IFoobar), typeof(Foobar));
            var proxy = (IFoobar)Activator.CreateInstance(proxyType, new object[] { _serviceProvider, new FakeInterceptorProvider(false, null) });
            var result = await proxy.InvokeAsTaskOfResult(1, 2);

            Assert.Null(FakeInterceptor.InvocationContext.Arguments);
            Assert.IsType<Foobar>(FakeInterceptor.InvocationContext.Target);
            Assert.Equal(typeof(Foobar).GetMethod(nameof(Foobar.InvokeAsTaskOfResult)).MakeGenericMethod(typeof(int)), FakeInterceptor.InvocationContext.Method);
            Assert.True(FakeInterceptor.Invoked);
            Assert.True(Foobar.Invoked);
            Assert.Equal(-1, result);
        }

        [Fact]
        public async void InvokeAsTaskOfResult_OverrideReturnValue()
        {
            FakeInterceptor.Reset();
            Foobar.Reset();
            var generator = new InterfaceProxyGenerator(new FakeInterceptorRegistrationProvider());
            var proxyType = generator.Generate(typeof(IFoobar), typeof(Foobar));
            var proxy = (IFoobar)Activator.CreateInstance(proxyType, new object[] { _serviceProvider, new FakeInterceptorProvider(true, Task.FromResult( 100)) });
            var result = await proxy.InvokeAsTaskOfResult(1, 2);
            Assert.Equal(100, result);
        }

        [Fact]
        public async void InvokeAsValueTask_CaptureArguments()
        {
            FakeInterceptor.Reset();
            Foobar.Reset();
            var generator = new InterfaceProxyGenerator(new FakeInterceptorRegistrationProvider());
            var proxyType = generator.Generate(typeof(IFoobar), typeof(Foobar));
            var proxy = (IFoobar)Activator.CreateInstance(proxyType, new object[] { _serviceProvider, new FakeInterceptorProvider(true, null) });
            await proxy.InvokeAsValueTask(1, 2);

            Assert.Equal(1, FakeInterceptor.InvocationContext.Arguments[0]);
            Assert.Equal(2, FakeInterceptor.InvocationContext.Arguments[1]);
            Assert.IsType<Foobar>(FakeInterceptor.InvocationContext.Target);
            Assert.Equal(typeof(Foobar).GetMethod(nameof(Foobar.InvokeAsValueTask)).MakeGenericMethod(typeof(int)), FakeInterceptor.InvocationContext.Method);
            Assert.True(FakeInterceptor.Invoked);
            Assert.True(Foobar.Invoked);
        }


        [Fact]
        public async void InvokeAsValueTask_Not_CaptureArguments()
        {
            FakeInterceptor.Reset();
            Foobar.Reset();
            var generator = new InterfaceProxyGenerator(new FakeInterceptorRegistrationProvider());
            var proxyType = generator.Generate(typeof(IFoobar), typeof(Foobar));
            var proxy = (IFoobar)Activator.CreateInstance(proxyType, new object[] { _serviceProvider, new FakeInterceptorProvider(true, null) });
            await proxy.InvokeAsValueTask(1, 2);

            Assert.Equal(1, FakeInterceptor.InvocationContext.Arguments[0]);
            Assert.Equal(2, FakeInterceptor.InvocationContext.Arguments[1]);
            Assert.IsType<Foobar>(FakeInterceptor.InvocationContext.Target);
            Assert.Equal(typeof(Foobar).GetMethod(nameof(Foobar.InvokeAsValueTask)).MakeGenericMethod(typeof(int)), FakeInterceptor.InvocationContext.Method);
            Assert.True(FakeInterceptor.Invoked);
            Assert.True(Foobar.Invoked);
        }

        [Fact]
        public async void InvokeAsValueTaskOfResult_CaptureArguments()
        {
            FakeInterceptor.Reset();
            Foobar.Reset();
            var generator = new InterfaceProxyGenerator(new FakeInterceptorRegistrationProvider());
            var proxyType = generator.Generate(typeof(IFoobar), typeof(Foobar));
            var proxy = (IFoobar)Activator.CreateInstance(proxyType, new object[] { _serviceProvider, new FakeInterceptorProvider(false, null) });
            var result = await proxy.InvokeAsValueTaskOfResult(1, 2);

            Assert.Null(FakeInterceptor.InvocationContext.Arguments);
            Assert.IsType<Foobar>(FakeInterceptor.InvocationContext.Target);
            Assert.Equal(typeof(Foobar).GetMethod(nameof(Foobar.InvokeAsValueTaskOfResult)).MakeGenericMethod(typeof(int)), FakeInterceptor.InvocationContext.Method);
            Assert.True(FakeInterceptor.Invoked);
            Assert.True(Foobar.Invoked);
            Assert.Equal(-1, result);
        }

        [Fact]
        public async void InvokeAsValueTaskOfResult_Not_CaptureArguments()
        {
            FakeInterceptor.Reset();
            Foobar.Reset();
            var generator = new InterfaceProxyGenerator(new FakeInterceptorRegistrationProvider());
            var proxyType = generator.Generate(typeof(IFoobar), typeof(Foobar));
            var proxy = (IFoobar)Activator.CreateInstance(proxyType, new object[] { _serviceProvider, new FakeInterceptorProvider(true, null) });
            var result = await proxy.InvokeAsValueTaskOfResult(1, 2);

            Assert.Equal(1, FakeInterceptor.InvocationContext.Arguments[0]);
            Assert.Equal(2, FakeInterceptor.InvocationContext.Arguments[1]);
            Assert.IsType<Foobar>(FakeInterceptor.InvocationContext.Target);
            Assert.Equal(typeof(Foobar).GetMethod(nameof(Foobar.InvokeAsValueTaskOfResult)).MakeGenericMethod(typeof(int)), FakeInterceptor.InvocationContext.Method);
            Assert.True(FakeInterceptor.Invoked);
            Assert.True(Foobar.Invoked);
            Assert.Equal(-1, result);
        }

        [Fact]
        public async void InvokeAsValueTaskOfResult_OverrideReturnValue()
        {
            FakeInterceptor.Reset();
            Foobar.Reset();
            var generator = new InterfaceProxyGenerator(new FakeInterceptorRegistrationProvider());
            var proxyType = generator.Generate(typeof(IFoobar), typeof(Foobar));
            var proxy = (IFoobar)Activator.CreateInstance(proxyType, new object[] { _serviceProvider, new FakeInterceptorProvider(true, new ValueTask<int>(100)) });
            var result = await proxy.InvokeAsValueTaskOfResult(1, 2);
            Assert.Equal(100, result);
        }

        public interface IFoobar
        {
            void InvokeAsVoid<T>(T x, T y) where T: IComparable<T>;
            int InvokeAsResult<T>(T x, T y) where T : IComparable<T>;
            Task InvokeAsTask<T>(T x, T y) where T : IComparable<T>;
            Task<int> InvokeAsTaskOfResult<T>(T x, T y) where T : IComparable<T>;
            ValueTask InvokeAsValueTask<T>(T x, T y) where T : IComparable<T>;
            ValueTask<int> InvokeAsValueTaskOfResult<T>(T x, T y) where T : IComparable<T>;
        }

        public class Bar
        { }

        public class Foobar : IFoobar
        {
            public Foobar(Bar bar)
            { }

            public static bool Invoked { get; private set; }
            public static void Reset() => Invoked = false;
            public int InvokeAsResult<T>(T x, T y) where T : IComparable<T>
            {
                Invoked = true;
                return x.CompareTo(y);
            }

            public async Task InvokeAsTask<T>(T x, T y) where T : IComparable<T>
            {
                Invoked = true;
                await Task.Delay(10);
                x.CompareTo(y);
            }

            public async Task<int> InvokeAsTaskOfResult<T>(T x, T y) where T : IComparable<T>
            {
                Invoked = true;
                await Task.Delay(10);
                return x.CompareTo(y);
            }

            public async ValueTask InvokeAsValueTask<T>(T x, T y) where T : IComparable<T>
            {
                Invoked = true;
                await Task.Delay(10);
            }

            public async ValueTask<int> InvokeAsValueTaskOfResult<T>(T x, T y) where T : IComparable<T>
            {
                Invoked = true;
                await Task.Delay(10);
                return x.CompareTo(y);
            }

            public void InvokeAsVoid<T>(T x, T y) where T : IComparable<T>
            {
                Invoked = true;
                x.CompareTo(y);
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
                yield return new InterceptorRegistration(_factory, typeof(Foobar).GetMethod(nameof(Foobar.InvokeAsVoid)), 1);
                yield return new InterceptorRegistration(_factory, typeof(Foobar).GetMethod(nameof(Foobar.InvokeAsResult)), 1);
                yield return new InterceptorRegistration(_factory, typeof(Foobar).GetMethod(nameof(Foobar.InvokeAsTask)), 1);
                yield return new InterceptorRegistration(_factory, typeof(Foobar).GetMethod(nameof(Foobar.InvokeAsTaskOfResult)), 1);
                yield return new InterceptorRegistration(_factory, typeof(Foobar).GetMethod(nameof(Foobar.InvokeAsValueTask)), 1);
                yield return new InterceptorRegistration(_factory, typeof(Foobar).GetMethod(nameof(Foobar.InvokeAsValueTaskOfResult)), 1);
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
