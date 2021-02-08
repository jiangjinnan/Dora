using Dora.Interception;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace App1
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var services = new ServiceCollection()
                .AddSingleton<ICalculator, Calculator>()
                .AddSingleton(typeof(ICalculator<>), typeof(Calculator<>))
                .AddSingleton<IServiceProviderAccessor, ServiceProviderAccessor>()
                .AddSingleton<IInterceptorBuilder, InterceptorBuilder>()
                .AddSingleton<IInterceptorProvider, InterceptorProvider>()
                .AddSingleton<IInterceptableProxyGenerator, InterfaceProxyGenerator>();
            services.TryAddEnumerable(ServiceDescriptor.Singleton< IInterceptorRegistrationProvider, AttributeInterceptorRegistrationProvider>());

            var calculator1 = new InterceptionContainer(services)
                .BuildServiceProvider()
                .GetRequiredService<ICalculator>();

            calculator1.DivideAsVoid(2, 1);
            Console.WriteLine();
            Console.WriteLine(calculator1.DivideAsResult(2, 1));
            Console.WriteLine();
            await calculator1.DivideAsTask(2, 1);
            Console.WriteLine();
            Console.WriteLine(await calculator1.DivideAsTaskOfResult(2, 1));
            Console.WriteLine();
            await calculator1.DivideAsValueTask(2, 1);
            Console.WriteLine();
            Console.WriteLine(await calculator1.DivideAsValueTaskOfResult(2, 1));
            Console.WriteLine();
            Console.WriteLine(calculator1.GenericDivideAsResult(2, 1));

            var calculator2 = new InterceptionContainer(services)
               .BuildServiceProvider()
               .GetRequiredService<ICalculator<int>>();
            Console.WriteLine();
            Console.WriteLine(calculator2.Add<double>(2, 1));
        }

        static async ValueTask InvokeAsync()
        {
            await Task.Delay(10);
            throw new Exception();
        }
    }

    public interface ICalculator<T>
    {
        TResult Add<TResult>(T x, T y);
    }

    public class Calculator<T> : ICalculator<T>
    {
        public TResult Add<TResult>(T x, T y)
        {
            Console.WriteLine("Add");
            return default;
        }
    }

    public interface ICalculator
    {
        void DivideAsVoid(int x, int y);
        int DivideAsResult(int x, int y);
        Task DivideAsTask(int x, int y);
        Task<int> DivideAsTaskOfResult(int x, int y);
        ValueTask DivideAsValueTask(int x, int y);
        ValueTask<int> DivideAsValueTaskOfResult(int x, int y);
        T GenericDivideAsResult<T>(T x, T y);
    }

    [TestInterceptor]
    public class Calculator: ICalculator
    {
        public int DivideAsResult(int x, int y)
        {
            Console.WriteLine("DivideAsResult");
            return  x / y;
        }

        public async Task DivideAsTask(int x, int y)
        {
            await Task.Delay(100);
            Console.WriteLine("DivideAsTask");
            _ = x / y;
        }

        public async Task<int> DivideAsTaskOfResult(int x, int y)
        {
            await Task.Delay(100);
            Console.WriteLine("DivideAsTaskOfResult");
            return x / y;
        }

        public async ValueTask DivideAsValueTask(int x, int y)
        {
            await Task.Delay(100);
            Console.WriteLine("DivideAsValueTask");
            _= x / y;
        }

        public async ValueTask<int> DivideAsValueTaskOfResult(int x, int y)
        {
            await Task.Delay(100);
            Console.WriteLine("DivideAsValueTaskOfResult");
           return x / y;
        }

        public void DivideAsVoid(int x, int y)
        {
            Console.WriteLine("DivideAsVoid");
            _ = x / y;
        }

        public T GenericDivideAsResult<T>(T x, T y)
        {
            Console.WriteLine("GenericDivideAsResult");
            return default;
        }
    }


    public class TestInterceptorAttribute : InterceptorAttribute
    {
        public async Task InvokeAsync(InvocationContext invocationContext)
        {
            Console.WriteLine("PreInvoke");
            await invocationContext.InvokeAsync();
            Console.WriteLine("PostInvoke");
        }

        protected override object CreateInterceptor(IServiceProvider serviceProvider)
        {
            return this;
        }
    }


    public class FakeInterceptor : IInterceptor
    {
        public bool AlterArguments => false;
        public bool CaptureArguments => true;
        public InterceptorDelegate Delegate => _next =>
        {
            return async invocationContext =>
            {
                Console.WriteLine("PreInvoke");
                await _next(invocationContext);
                Console.WriteLine("PostInvoke");
            };
        };
    }

    //public class FakeInterceptorProvider : IInterceptorProvider
    //{
    //    public IInterceptor GetInterceptor(MethodInfo method) => new FakeInterceptor();
    //}

    //public class FakeInterceptorRegistrationProvider : IInterceptorRegistrationProvider
    //{
    //    public IEnumerable<InterceptorRegistration> Registrations => new InterceptorRegistration[] {
    //        new InterceptorRegistration(_=> new object(), typeof(Calculator).GetMethod("DivideAsVoid"), 0),
    //        new InterceptorRegistration(_=> new object(), typeof(Calculator).GetMethod("DivideAsResult"), 0),
    //        new InterceptorRegistration(_=> new object(), typeof(Calculator).GetMethod("DivideAsTask"), 0),
    //        new InterceptorRegistration(_=> new object(), typeof(Calculator).GetMethod("DivideAsTaskOfResult"), 0),
    //        new InterceptorRegistration(_=> new object(), typeof(Calculator).GetMethod("DivideAsValueTask"), 0),
    //        new InterceptorRegistration(_=> new object(), typeof(Calculator).GetMethod("DivideAsValueTaskOfResult"), 0),
    //        new InterceptorRegistration(_=> new object(), typeof(Calculator).GetMethod("GenericDivideAsResult"), 0),
    //        new InterceptorRegistration(_=> new object(), typeof(Calculator<>).GetMethod("Add"), 0)
    //    };

    //    public IEnumerable<InterceptorRegistration> GetRegistrations(Type serviceType)
    //    {
    //        if (serviceType == typeof(Calculator))
    //        {
    //            return new InterceptorRegistration[] {
    //                new InterceptorRegistration(_ => new object(), typeof(Calculator).GetMethod("DivideAsVoid"), 0),
    //                new InterceptorRegistration(_ => new object(), typeof(Calculator).GetMethod("DivideAsResult"), 0),
    //                new InterceptorRegistration(_ => new object(), typeof(Calculator).GetMethod("DivideAsTask"), 0),
    //                new InterceptorRegistration(_ => new object(), typeof(Calculator).GetMethod("DivideAsTaskOfResult"), 0),
    //                new InterceptorRegistration(_ => new object(), typeof(Calculator).GetMethod("DivideAsValueTask"), 0),
    //                new InterceptorRegistration(_ => new object(), typeof(Calculator).GetMethod("DivideAsValueTaskOfResult"), 0),
    //                new InterceptorRegistration(_ => new object(), typeof(Calculator).GetMethod("GenericDivideAsResult"), 0)
    //            };
    //        }

    //        if (serviceType == typeof(Calculator<>))
    //        {
    //            return new InterceptorRegistration[] {
    //                new InterceptorRegistration(_=> new object(), typeof(Calculator<>).GetMethod("Add"), 0)
    //            };
    //        }

    //        return Array.Empty<InterceptorRegistration>();
    //    }
    //}
}
