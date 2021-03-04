using Dora.Interception;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace Interception.Test
{
    public class InterfaceProxyGeneratorFixture__GenericType
    {
        private readonly IServiceProvider _serviceProvider = new ServiceCollection()
            .AddSingleton<Bar>()
            .BuildServiceProvider();

        [Fact]
        public void InvokeAsVoid_CaptureArguments()
        {
            FakeInterceptor.Reset();
            Foobar<int>.Reset();
            var generator = new InterfaceProxyGenerator(new FakeInterceptorRegistrationProvider());
            var proxyType = generator.Generate(typeof(IFoobar<>), typeof(Foobar<>));
            var proxy = (IFoobar<int>)Activator.CreateInstance(proxyType.MakeGenericType(typeof(int)), new object[] { _serviceProvider, new FakeInterceptorProvider(true, null)});
            proxy.InvokeAsVoid(1, 2);

            Assert.Equal(1, FakeInterceptor.InvocationContext.Arguments[0]);
            Assert.Equal(2, FakeInterceptor.InvocationContext.Arguments[1]);
            Assert.IsType<Foobar<int>>(FakeInterceptor.InvocationContext.Target);
            Assert.Equal(typeof(Foobar<int>).GetMethod(nameof(Foobar<int>.InvokeAsVoid)).MakeGenericMethod(typeof(int)), FakeInterceptor.InvocationContext.Method);
            Assert.True(FakeInterceptor.Invoked);
            Assert.True(Foobar<int>.Invoked);
        }

        [Fact]
        public void InvokeAsVoid_Not_CaptureArguments()
        {
            FakeInterceptor.Reset();
            Foobar<int>.Reset();
            var generator = new InterfaceProxyGenerator(new FakeInterceptorRegistrationProvider());
            var proxyType = generator.Generate(typeof(IFoobar<>), typeof(Foobar<>));
            var proxy = (IFoobar<int>)Activator.CreateInstance(proxyType.MakeGenericType(typeof(int)), new object[] { _serviceProvider, new FakeInterceptorProvider(false, null) });
            proxy.InvokeAsVoid(1, 2);

            Assert.Null(FakeInterceptor.InvocationContext.Arguments);
            Assert.IsType<Foobar<int>>(FakeInterceptor.InvocationContext.Target);
            Assert.Equal(typeof(Foobar<int>).GetMethod(nameof(Foobar<int>.InvokeAsVoid)).MakeGenericMethod(typeof(int)), FakeInterceptor.InvocationContext.Method);
            Assert.True(FakeInterceptor.Invoked);
            Assert.True(Foobar<int>.Invoked);
        }

        [Fact]
        public void InvokeAsResult_CaptureArguments()
        {
            FakeInterceptor.Reset();
            Foobar<int>.Reset();
            var generator = new InterfaceProxyGenerator(new FakeInterceptorRegistrationProvider());
            var proxyType = generator.Generate(typeof(IFoobar<>), typeof(Foobar<>));
            var proxy = (IFoobar<int>)Activator.CreateInstance(proxyType.MakeGenericType(typeof(int)), new object[] { _serviceProvider, new FakeInterceptorProvider(true, null) });
            var result = proxy.InvokeAsResult(1, 2);

            Assert.Equal(1, FakeInterceptor.InvocationContext.Arguments[0]);
            Assert.Equal(2, FakeInterceptor.InvocationContext.Arguments[1]);
            Assert.IsType<Foobar<int>>(FakeInterceptor.InvocationContext.Target);
            Assert.Equal(typeof(Foobar<int>).GetMethod(nameof(Foobar<int>.InvokeAsResult)).MakeGenericMethod(typeof(int)), FakeInterceptor.InvocationContext.Method);
            Assert.True(FakeInterceptor.Invoked);
            Assert.True(Foobar<int>.Invoked);
            Assert.Equal(0,result);
        }

        [Fact]
        public void InvokeAsResult_Not_CaptureArguments()
        {
            FakeInterceptor.Reset();
            Foobar<int>.Reset();
            var generator = new InterfaceProxyGenerator(new FakeInterceptorRegistrationProvider());
            var proxyType = generator.Generate(typeof(IFoobar<>), typeof(Foobar<>));
            var proxy = (IFoobar<int>)Activator.CreateInstance(proxyType.MakeGenericType(typeof(int)), new object[] { _serviceProvider, new FakeInterceptorProvider(false, null) });
            var result = proxy.InvokeAsResult(1, 2);

            Assert.Null(FakeInterceptor.InvocationContext.Arguments);
            Assert.Equal(typeof(Foobar<int>).GetMethod(nameof(Foobar<int>.InvokeAsResult)).MakeGenericMethod(typeof(int)), FakeInterceptor.InvocationContext.Method);
            Assert.True(FakeInterceptor.Invoked);
            Assert.True(Foobar<int>.Invoked);
            Assert.Equal(0, result);
        }

        [Fact]
        public void InvokeAsResult_OverrideReturnValue()
        {
            FakeInterceptor.Reset();
            Foobar<int>.Reset();
            var generator = new InterfaceProxyGenerator(new FakeInterceptorRegistrationProvider());
            var proxyType = generator.Generate(typeof(IFoobar<>), typeof(Foobar<>));
            var proxy = (IFoobar<int>)Activator.CreateInstance(proxyType.MakeGenericType(typeof(int)), new object[] { _serviceProvider, new FakeInterceptorProvider(true, 100) });
            var result = proxy.InvokeAsResult(1, 2);
            Assert.Equal(100, result);
        }

        [Fact]
        public async void InvokeAsTask_CaptureArguments()
        {
            FakeInterceptor.Reset();
            Foobar<int>.Reset();
            var generator = new InterfaceProxyGenerator(new FakeInterceptorRegistrationProvider());
            var proxyType = generator.Generate(typeof(IFoobar<>), typeof(Foobar<>));
            var proxy = (IFoobar<int>)Activator.CreateInstance(proxyType.MakeGenericType(typeof(int)), new object[] { _serviceProvider, new FakeInterceptorProvider(true, null) });
            await proxy.InvokeAsTask(1, 2);

            Assert.Equal(1, FakeInterceptor.InvocationContext.Arguments[0]);
            Assert.Equal(2, FakeInterceptor.InvocationContext.Arguments[1]);
            Assert.IsType<Foobar<int>>(FakeInterceptor.InvocationContext.Target);
            Assert.Equal(typeof(Foobar<int>).GetMethod(nameof(Foobar<int>.InvokeAsTask)).MakeGenericMethod(typeof(int)), FakeInterceptor.InvocationContext.Method);
            Assert.True(FakeInterceptor.Invoked);
            Assert.True(Foobar<int>.Invoked);
        }

        [Fact]
        public async void InvokeAsTask_Not_CaptureArguments()
        {
            FakeInterceptor.Reset();
            Foobar<int>.Reset();
            var generator = new InterfaceProxyGenerator(new FakeInterceptorRegistrationProvider());
            var proxyType = generator.Generate(typeof(IFoobar<>), typeof(Foobar<>));
            var proxy = (IFoobar<int>)Activator.CreateInstance(proxyType.MakeGenericType(typeof(int)), new object[] { _serviceProvider, new FakeInterceptorProvider(false, null) });
            await proxy.InvokeAsTask(1, 2);

            Assert.Null(FakeInterceptor.InvocationContext.Arguments);
            Assert.IsType<Foobar<int>>(FakeInterceptor.InvocationContext.Target);
            Assert.Equal(typeof(Foobar<int>).GetMethod(nameof(Foobar<int>.InvokeAsTask)).MakeGenericMethod(typeof(int)), FakeInterceptor.InvocationContext.Method);
            Assert.True(FakeInterceptor.Invoked);
            Assert.True(Foobar<int>.Invoked);
        }

        [Fact]
        public async void InvokeAsTaskOfResult_CaptureArguments()
        {
            FakeInterceptor.Reset();
            Foobar<int>.Reset();
            var generator = new InterfaceProxyGenerator(new FakeInterceptorRegistrationProvider());
            var proxyType = generator.Generate(typeof(IFoobar<>), typeof(Foobar<>));
            var proxy = (IFoobar<int>)Activator.CreateInstance(proxyType.MakeGenericType(typeof(int)), new object[] { _serviceProvider, new FakeInterceptorProvider(true, null) });
            var result = await proxy.InvokeAsTaskOfResult(1, 2);

            Assert.Equal(1, FakeInterceptor.InvocationContext.Arguments[0]);
            Assert.Equal(2, FakeInterceptor.InvocationContext.Arguments[1]);
            Assert.IsType<Foobar<int>>(FakeInterceptor.InvocationContext.Target);
            Assert.Equal(typeof(Foobar<int>).GetMethod(nameof(Foobar<int>.InvokeAsTaskOfResult)).MakeGenericMethod(typeof(int)), FakeInterceptor.InvocationContext.Method);
            Assert.True(FakeInterceptor.Invoked);
            Assert.True(Foobar<int>.Invoked);
            Assert.Equal(0, result);
        }

        [Fact]
        public async void InvokeAsTaskOfResult_Not_CaptureArguments()
        {
            FakeInterceptor.Reset();
            Foobar<int>.Reset();
            var generator = new InterfaceProxyGenerator(new FakeInterceptorRegistrationProvider());
            var proxyType = generator.Generate(typeof(IFoobar<>), typeof(Foobar<>));
            var proxy = (IFoobar<int>)Activator.CreateInstance(proxyType.MakeGenericType(typeof(int)), new object[] { _serviceProvider, new FakeInterceptorProvider(false, null) });
            var result = await proxy.InvokeAsTaskOfResult(1, 2);

            Assert.Null(FakeInterceptor.InvocationContext.Arguments);
            Assert.IsType<Foobar<int>>(FakeInterceptor.InvocationContext.Target);
            Assert.Equal(typeof(Foobar<int>).GetMethod(nameof(Foobar<int>.InvokeAsTaskOfResult)).MakeGenericMethod(typeof(int)), FakeInterceptor.InvocationContext.Method);
            Assert.True(FakeInterceptor.Invoked);
            Assert.True(Foobar<int>.Invoked);
            Assert.Equal(0, result);
        }

        [Fact]
        public async void InvokeAsTaskOfResult_OverrideReturnValue()
        {
            FakeInterceptor.Reset();
            Foobar<int>.Reset();
            var generator = new InterfaceProxyGenerator(new FakeInterceptorRegistrationProvider());
            var proxyType = generator.Generate(typeof(IFoobar<>), typeof(Foobar<>));
            var proxy = (IFoobar<int>)Activator.CreateInstance(proxyType.MakeGenericType(typeof(int)), new object[] { _serviceProvider, new FakeInterceptorProvider(true, Task.FromResult( 100)) });
            var result = await proxy.InvokeAsTaskOfResult(1, 2);
            Assert.Equal(100, result);
        }

        [Fact]
        public async void InvokeAsValueTask_CaptureArguments()
        {
            FakeInterceptor.Reset();
            Foobar<int>.Reset();
            var generator = new InterfaceProxyGenerator(new FakeInterceptorRegistrationProvider());
            var proxyType = generator.Generate(typeof(IFoobar<>), typeof(Foobar<>));
            var proxy = (IFoobar<int>)Activator.CreateInstance(proxyType.MakeGenericType(typeof(int)), new object[] { _serviceProvider, new FakeInterceptorProvider(true, null) });
            await proxy.InvokeAsValueTask(1, 2);

            Assert.Equal(1, FakeInterceptor.InvocationContext.Arguments[0]);
            Assert.Equal(2, FakeInterceptor.InvocationContext.Arguments[1]);
            Assert.IsType<Foobar<int>>(FakeInterceptor.InvocationContext.Target);
            Assert.Equal(typeof(Foobar<int>).GetMethod(nameof(Foobar<int>.InvokeAsValueTask)).MakeGenericMethod(typeof(int)), FakeInterceptor.InvocationContext.Method);
            Assert.True(FakeInterceptor.Invoked);
            Assert.True(Foobar<int>.Invoked);
        }


        [Fact]
        public async void InvokeAsValueTask_Not_CaptureArguments()
        {
            FakeInterceptor.Reset();
            Foobar<int>.Reset();
            var generator = new InterfaceProxyGenerator(new FakeInterceptorRegistrationProvider());
            var proxyType = generator.Generate(typeof(IFoobar<>), typeof(Foobar<>));
            var proxy = (IFoobar<int>)Activator.CreateInstance(proxyType.MakeGenericType(typeof(int)), new object[] { _serviceProvider, new FakeInterceptorProvider(true, null) });
            await proxy.InvokeAsValueTask(1, 2);

            Assert.Equal(1, FakeInterceptor.InvocationContext.Arguments[0]);
            Assert.Equal(2, FakeInterceptor.InvocationContext.Arguments[1]);
            Assert.IsType<Foobar<int>>(FakeInterceptor.InvocationContext.Target);
            Assert.Equal(typeof(Foobar<int>).GetMethod(nameof(Foobar<int>.InvokeAsValueTask)).MakeGenericMethod(typeof(int)), FakeInterceptor.InvocationContext.Method);
            Assert.True(FakeInterceptor.Invoked);
            Assert.True(Foobar<int>.Invoked);
        }

        [Fact]
        public async void InvokeAsValueTaskOfResult_CaptureArguments()
        {
            FakeInterceptor.Reset();
            Foobar<int>.Reset();
            var generator = new InterfaceProxyGenerator(new FakeInterceptorRegistrationProvider());
            var proxyType = generator.Generate(typeof(IFoobar<>), typeof(Foobar<>));
            var proxy = (IFoobar<int>)Activator.CreateInstance(proxyType.MakeGenericType(typeof(int)), new object[] { _serviceProvider, new FakeInterceptorProvider(false, null) });
            var result = await proxy.InvokeAsValueTaskOfResult(1, 2);

            Assert.Null(FakeInterceptor.InvocationContext.Arguments);
            Assert.IsType<Foobar<int>>(FakeInterceptor.InvocationContext.Target);
            Assert.Equal(typeof(Foobar<int>).GetMethod(nameof(Foobar<int>.InvokeAsValueTaskOfResult)).MakeGenericMethod(typeof(int)), FakeInterceptor.InvocationContext.Method);
            Assert.True(FakeInterceptor.Invoked);
            Assert.True(Foobar<int>.Invoked);
            Assert.Equal(0, result);
        }

        [Fact]
        public async void InvokeAsValueTaskOfResult_Not_CaptureArguments()
        {
            FakeInterceptor.Reset();
            Foobar<int>.Reset();
            var generator = new InterfaceProxyGenerator(new FakeInterceptorRegistrationProvider());
            var proxyType = generator.Generate(typeof(IFoobar<>), typeof(Foobar<>));
            var proxy = (IFoobar<int>)Activator.CreateInstance(proxyType.MakeGenericType(typeof(int)), new object[] { _serviceProvider, new FakeInterceptorProvider(true, null) });
            var result = await proxy.InvokeAsValueTaskOfResult(1, 2);

            Assert.Equal(1, FakeInterceptor.InvocationContext.Arguments[0]);
            Assert.Equal(2, FakeInterceptor.InvocationContext.Arguments[1]);
            Assert.IsType<Foobar<int>>(FakeInterceptor.InvocationContext.Target);
            Assert.Equal(typeof(Foobar<int>).GetMethod(nameof(Foobar<int>.InvokeAsValueTaskOfResult)).MakeGenericMethod(typeof(int)), FakeInterceptor.InvocationContext.Method);
            Assert.True(FakeInterceptor.Invoked);
            Assert.True(Foobar<int>.Invoked);
            Assert.Equal(0, result);
        }

        [Fact]
        public async void InvokeAsValueTaskOfResult_OverrideReturnValue()
        {
            FakeInterceptor.Reset();
            Foobar<int>.Reset();
            var generator = new InterfaceProxyGenerator(new FakeInterceptorRegistrationProvider());
            var proxyType = generator.Generate(typeof(IFoobar<>), typeof(Foobar<>));
            var proxy = (IFoobar<int>)Activator.CreateInstance(proxyType.MakeGenericType(typeof(int)), new object[] { _serviceProvider, new FakeInterceptorProvider(true, new ValueTask<int>(100)) });
            var result = await proxy.InvokeAsValueTaskOfResult(1, 2);
            Assert.Equal(100, result);
        }

        public interface IFoobar<TResult> where TResult: IComparable<TResult>, new()
        {
            void InvokeAsVoid<T>(T x, T y);
            TResult InvokeAsResult<T>(T x, T y);
            Task InvokeAsTask<T>(T x, T y);
            Task<TResult> InvokeAsTaskOfResult<T>(T x, T y);
            ValueTask InvokeAsValueTask<T>(T x, T y);
            ValueTask<TResult> InvokeAsValueTaskOfResult<T>(T x, T y);
        }

        public class Bar
        { }

        public class Foobar<TResult> : IFoobar<TResult> where TResult : IComparable<TResult>, new()
        {
            public Foobar(Bar bar)
            { }

            public static bool Invoked { get; private set; }
            public static void Reset() => Invoked = false;

            public TResult InvokeAsResult<T>(T x, T y)
            {
                Invoked = true;
                return default;
            }

            public  Task InvokeAsTask<T>(T x, T y)
            {
                Invoked = true;
                return Task.Delay(10);
            }

            public async Task<TResult> InvokeAsTaskOfResult<T>(T x, T y)
            {
                Invoked = true;
                await Task.Delay(10);
                return default;
            }

            public async ValueTask InvokeAsValueTask<T>(T x, T y)
            {
                Invoked = true;
                await Task.Delay(10);
            }

            public async ValueTask<TResult> InvokeAsValueTaskOfResult<T>(T x, T y)
            {
                Invoked = true;
                await Task.Delay(10);
                return default;
            }

            public void InvokeAsVoid<T>(T x, T y)
            {
                Invoked = true;
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
                yield return new InterceptorRegistration(_factory, typeof(Foobar<>).GetMethod(nameof(Foobar<int>.InvokeAsVoid)), 1);
                yield return new InterceptorRegistration(_factory, typeof(Foobar<>).GetMethod(nameof(Foobar<int>.InvokeAsResult)), 1);
                yield return new InterceptorRegistration(_factory, typeof(Foobar<>).GetMethod(nameof(Foobar<int>.InvokeAsTask)), 1);
                yield return new InterceptorRegistration(_factory, typeof(Foobar<>).GetMethod(nameof(Foobar<int>.InvokeAsTaskOfResult)), 1);
                yield return new InterceptorRegistration(_factory, typeof(Foobar<>).GetMethod(nameof(Foobar<int>.InvokeAsValueTask)), 1);
                yield return new InterceptorRegistration(_factory, typeof(Foobar<>).GetMethod(nameof(Foobar<int>.InvokeAsValueTaskOfResult)), 1);
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
