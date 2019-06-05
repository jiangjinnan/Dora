using Castle.DynamicProxy;
using Dora.DynamicProxy;
using System.Collections.Generic;
using System.Reflection;

namespace Dora.Interception.Castle
{
    internal class DynamicProxyInvocationContext : InvocationContext
    {
        private IInvocation _invocation;

        public DynamicProxyInvocationContext(IInvocation invocation)
        {
            _invocation = invocation;
        }

        public override MethodInfo Method => _invocation.Method;

        public override object Proxy => _invocation.Proxy;

        public override object Target => _invocation.InvocationTarget;

        public override object[] Arguments => _invocation.Arguments;

        public override object ReturnValue { get => _invocation.ReturnValue; set => _invocation.ReturnValue = value; }

        public override IDictionary<string, object> Properties => new Dictionary<string, object>();
    }
}
