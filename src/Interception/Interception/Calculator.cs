using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Dora.Interception
{
    internal interface ICalculator<T>
    {
        TResult Add<TResult>(T x, T y);

        void AddRefAndOutArguments<TResult>(ref T x, ref T y, out TResult result);
    }

    internal class Calculator<T> : ICalculator<T>
    {
        public TResult Add<TResult>(T x, T y)
        {
            throw new NotImplementedException();
        }

        public void AddRefAndOutArguments<TResult>(ref T x, ref T y, out TResult result)
        {
            throw new NotImplementedException();
        }
    }

    internal class CalculatorProxy<T> : ICalculator<T>
    {
        private readonly Calculator<T> _target;
        private readonly IInterceptorProvider _interceptorProvider;
        public CalculatorProxy(IServiceProvider serviceProvider, IInterceptorProvider interceptorProvider)
        {
            _target = ActivatorUtilities.CreateInstance<Calculator<T>>(serviceProvider);
            _interceptorProvider = interceptorProvider;
        }

        public TResult Add<TResult>(T x, T y)
        {
            MethodInfo currentMethod = null;
            var interceptor = _interceptorProvider.GetInterceptor(currentMethod);
            var invocationContext = interceptor.CaptureArguments
                ? new InvocationContext(_target, currentMethod, new object[] { x, y })
                : new InvocationContext(_target, currentMethod);
            InvokerDelegate next = new AddClosure<T, TResult>(_target, x, y, invocationContext.Arguments).InvokeAsync;
            var task = interceptor.Delegate(next)(invocationContext);
            return ProxyGeneratorHelper.GetResult<TResult>(task, invocationContext);
        }

        public void AddRefAndOutArguments<TResult>(ref T x, ref T y, out TResult result)
        {
            MethodInfo currentMethod = null;
            var interceptor = _interceptorProvider.GetInterceptor(currentMethod);
            var arguments = new object[] { x, y, null };
            var invocationContext = new InvocationContext(_target, currentMethod, arguments);
            InvokerDelegate next = new AddRefAndOutArgumentsClosure<T, TResult>(_target, invocationContext.Arguments).InvokeAsync;
            var task = interceptor.Delegate(next)(invocationContext);
            task.Wait();

            x = (T)arguments[0];
            y = (T)arguments[1];
            result = (TResult)arguments[2];
        }
    }

    internal class AddClosure<T, TResult>
    {
        private readonly Calculator<T> _target;
        private readonly T _x;
        private readonly T _y;
        private object[] _arguments;

        public AddClosure(Calculator<T> target, T x, T y, object[] arguments)
        {
            _target = target;
            _x = x;
            _y = y;
            _arguments = arguments;
        }

        public Task InvokeAsync(InvocationContext invocationContext)
        {
            TResult result;
            if (_arguments == null)
            {
                result = _target.Add<TResult>(_x, _y);
            }
            else
            {
                result = _target.Add<TResult>((T)_arguments[0], (T)_arguments[1]);
            }
            invocationContext.SetReturnValue(result);
            return Task.CompletedTask;
        }
    }
    internal class AddRefAndOutArgumentsClosure<T, TResult>
    {
        private readonly Calculator<T> _target;
        private T _x;
        private T _y;
        private object[] _arguments;

        public AddRefAndOutArgumentsClosure(Calculator<T> target, object[] arguments)
        {
            _target = target;
            _arguments = arguments;
        }

        public Task InvokeAsync(InvocationContext invocationContext)
        {
            _x = (T)_arguments[0];
            _y = (T)_arguments[1];

            _target.AddRefAndOutArguments<TResult>(ref _x, ref _y, out var result);

            _arguments[0] = _x;
            _arguments[1] = _y;
            _arguments[2] = result;

            return Task.CompletedTask;
        }
    }
}