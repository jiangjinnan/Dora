using System;
using System.Reflection;

namespace Dora.Interception
{
    public abstract class InvocationContext
    {
        public abstract object[] Arguments { get; }

        public abstract Type[] GenericArguments { get; }

        public abstract object InvocationTarget { get; }

        public abstract MethodInfo Method { get; }

        public abstract MethodInfo MethodInvocationTarget { get; }

        public abstract object Proxy { get; }

        public abstract object ReturnValue { get; set; }

        public abstract Type TargetType { get; }


        public abstract object GetArgumentValue(int index);

        public abstract void SetArgumentValue(int index, object value);
    }
}
