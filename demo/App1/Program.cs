using Dora.Interception;
using Microsoft.Extensions.DependencyInjection;
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
                .AddSingleton<IInterceptorProvider, FakeInterceptorProvider>()
                .AddSingleton<IInterceptableProxyGenerator, InterceptableProxyGenerator>()
                .AddSingleton<IInterceptorRegistrationProvider, FakeInterceptorRegistrationProvider>();
            var calculator = new InterceptionContainer(services)
                .BuildServiceProvider()
                .GetRequiredService<ICalculator>();

            calculator.DivideAsVoid(2, 1);
            Console.WriteLine();
            Console.WriteLine(calculator.DivideAsResult(2, 1));
            Console.WriteLine();
            await calculator.DivideAsTask(2, 1);
            Console.WriteLine();
            Console.WriteLine(await calculator.DivideAsTaskOfResult(2, 1));
            Console.WriteLine();
            await calculator.DivideAsValueTask(2, 1);
            Console.WriteLine();
            Console.WriteLine(await calculator.DivideAsValueTaskOfResult(2, 1));
        }

        static async ValueTask InvokeAsync()
        {
            await Task.Delay(10);
            throw new Exception();
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
    }

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

    public class FakeInterceptorProvider : IInterceptorProvider
    {
        public IInterceptor GetInterceptor(MethodInfo method) => new FakeInterceptor();
    }

    public class FakeInterceptorRegistrationProvider : IInterceptorRegistrationProvider
    {
        public IEnumerable<InterceptorRegistration> Registrations => new InterceptorRegistration[] {
            new InterceptorRegistration(typeof(Calculator), typeof(Calculator).GetMethod("DivideAsVoid"), 0),
            new InterceptorRegistration(typeof(Calculator), typeof(Calculator).GetMethod("DivideAsResult"), 0),
            new InterceptorRegistration(typeof(Calculator), typeof(Calculator).GetMethod("DivideAsTask"), 0),
            new InterceptorRegistration(typeof(Calculator), typeof(Calculator).GetMethod("DivideAsTaskOfResult"), 0),
            new InterceptorRegistration(typeof(Calculator), typeof(Calculator).GetMethod("DivideAsValueTask"), 0),
            new InterceptorRegistration(typeof(Calculator), typeof(Calculator).GetMethod("DivideAsValueTaskOfResult"), 0),
            //new InterceptorRegistration(typeof(Calculator), typeof(Calculator).GetMethod("DivideAsOut"), 0)
        };
    }
}
