using Dora.Interception;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Interception.Test
{
    public class Calculator
    {
        public Calculator(string args)
        { }
        public virtual void AddAsVoid(int x, int y)
        { }
    }

    public class CalculatorProxy : Calculator
    {
        private readonly IInterceptorProvider _interceptorProvider;

        public CalculatorProxy(string args,IInterceptorProvider interceptorProvider):base(args)
        {
            _interceptorProvider = interceptorProvider;
        }

        public override void AddAsVoid(int x, int y)
        {
            MethodInfo currentMethod = null;
            var interceptor = _interceptorProvider.GetInterceptor(currentMethod);
            var invocationContext = interceptor.CaptureArguments
                ? new InvocationContext(this, currentMethod, new object[] { x, y })
                : new InvocationContext(this, currentMethod);
            var closure = new AddAsVoidClosure(this, x, y, invocationContext.Arguments);
            interceptor.Delegate(closure.InvokeAsync)(invocationContext).Wait();
        }
    }

    public class AddAsVoidClosure
    {
        private readonly Calculator _target;
        private readonly int _x;
        private readonly int _y;
        private readonly object[] _arguments;

        public AddAsVoidClosure(Calculator target, int x, int y, object[] arguments)
        {
            _target = target;
            if (arguments == null)
            {
                _x = x;
                _y = y;
            }
            else
            {
                _arguments = arguments;
            }
        }

        public Task InvokeAsync(InvocationContext invocationContext)
        {
            if (_arguments == null)
            {
                _target.AddAsVoid(_x, _y);
            }
            else
            {
                _target.AddAsVoid((int)_arguments[0], (int)_arguments[1]);
            }
            return Task.CompletedTask;
        }
    }
}
