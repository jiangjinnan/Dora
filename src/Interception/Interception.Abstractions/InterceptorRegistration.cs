using System;
using System.Reflection;

namespace Dora.Interception
{
    public class InterceptorRegistration
    {
        public Type InterceptorType { get; }
        public MethodInfo Target { get; }
        public int Order { get; }
        public InterceptorRegistration(Type interceptorType, MethodInfo target, int order)
        {
            InterceptorType = interceptorType ?? throw new ArgumentNullException(nameof(interceptorType));
            Target = target ?? throw new ArgumentNullException(nameof(target));
            Order = order;
        }
    }
}
