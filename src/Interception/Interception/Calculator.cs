using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dora.Interception
{
    internal interface ICalculator<T>
    {
        TResult Add<TResult>(T x, T y);
    }

    internal class Calculator<T> : ICalculator<T>
    {
        public TResult Add<TResult>(T x, T y)
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

        internal class AddClosure2<T, TResult>
        {
            private readonly Calculator<T> _target;
            private readonly T _x;
            private readonly T _y;
            private object[] _arguments;

            public AddClosure2(Calculator<T> target, T x, T y, object[] arguments)
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
    }
}
