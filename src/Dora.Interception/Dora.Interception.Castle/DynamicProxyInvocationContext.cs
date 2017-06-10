using Castle.DynamicProxy;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Dora.Interception.Castle
{
    internal class DynamicProxyInvocationContext : InvocationContext
    {
        private IInvocation _invocation;

        public DynamicProxyInvocationContext(IInvocation invocation)
        {
            _invocation = invocation;
        }

        public override object[] Arguments
        {
            get { return _invocation.Arguments; }
        }

        public override Type[] GenericArguments
        {
            get { return _invocation.GenericArguments; }
        }

        public override object InvocationTarget
        {
            get { return _invocation.InvocationTarget; }
        }

        public override MethodInfo Method
        {
            get { return _invocation.Method; }
        }

        public override MethodInfo MethodInvocationTarget
        {
            get { return _invocation.MethodInvocationTarget; }
        }

        public override object Proxy
        {
            get { return _invocation.Proxy; }
        }

        public override object ReturnValue
        {
            get { return _invocation.ReturnValue; }
            set { _invocation.ReturnValue = value; }
        }

        public override Type TargetType
        {
            get { return _invocation.TargetType; }
        }

        public override object GetArgumentValue(int index)
        {
            return _invocation.GetArgumentValue(index);
        }

        public override void SetArgumentValue(int index, object value)
        {
            _invocation.SetArgumentValue(index, value);
        }

        public async Task ProceedAsync()
        {
            await Task.Run(()=>_invocation.Proceed());
            await ((this.ReturnValue as Task) ?? Task.CompletedTask);
        }
    }
}
