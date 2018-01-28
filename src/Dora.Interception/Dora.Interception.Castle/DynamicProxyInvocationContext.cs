using Castle.DynamicProxy;
using Dora.DynamicProxy;
using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Dora.Interception.Castle
{
    internal class DynamicProxyInvocationContext : InvocationContext
    {
        private IInvocation _invocation;

        public DynamicProxyInvocationContext(IInvocation invocation)
        {
            _invocation = invocation;
        }

        public override MethodBase Method => _invocation.Method;

        public override object Proxy => _invocation.Proxy;

        public override object Target => _invocation.InvocationTarget;

        public override object[] Arguments => _invocation.Arguments;

        public override object ReturnValue { get => _invocation.ReturnValue; set => _invocation.ReturnValue = value; }

        public override IDictionary<string, object> ExtendedProperties => new Dictionary<string, object>();
    }
}
