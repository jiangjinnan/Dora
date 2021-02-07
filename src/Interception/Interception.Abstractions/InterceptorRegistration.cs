using System;
using System.Reflection;

namespace Dora.Interception
{
    public class InterceptorRegistration
    {
        public Func<IServiceProvider, object> InterceptorFactory { get; }
        public MethodInfo Target { get; }
        public int Order { get; }
        public InterceptorRegistration(Func<IServiceProvider, object> interceptorFactory, MethodInfo target, int order)
        {
            InterceptorFactory = interceptorFactory ?? throw new ArgumentNullException(nameof(interceptorFactory));
            Target = target ?? throw new ArgumentNullException(nameof(target));
            Order = order;
        }
    }
}