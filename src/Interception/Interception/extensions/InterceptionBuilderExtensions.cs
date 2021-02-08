using Dora.Interception;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class InterceptionBuilderExtensions
    {
        public static InterceptionBuilder RegisterInterceptors(this InterceptionBuilder builder, Action<IInterceptorRegistry> register)
        {
            var registry = new InterceptorRegistry();
            register(registry);
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IInterceptorRegistrationProvider>(registry));
            return builder;
        }
    }
}
