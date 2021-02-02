using System;
using System.Reflection;

namespace Dora.Interception
{
    public class InvocationContext
    {
        public object Target { get; }
        public MethodInfo Method { get; }
        public object[] Arguments { get; }
        public object ReturnValue { get; private set; }

        public InvocationContext(object target, MethodInfo method, object[] arguments = null)
        {
            Target = target ?? throw new ArgumentNullException(nameof(target));
            Method = method ?? throw new ArgumentNullException(nameof(method));
            Arguments = arguments;
        }

        public void SetReturnValue<T>(T result) => ReturnValue = result;
        public T GetReturnValue<T>() => (T)ReturnValue;
    }
}
