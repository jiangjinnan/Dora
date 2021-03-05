using Dora.Interception;
using Dora.Primitives;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class InterceptionBuilderExtensions
    {
        public static InterceptionBuilder RegisterInterceptors(this InterceptionBuilder builder, Action<IInterceptorRegistry> register)
        {
            Guard.ArgumentNotNull(builder, nameof(builder));
            Guard.ArgumentNotNull(register, nameof(register));
            var registry = new InterceptorRegistry();
            register(registry);
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IInterceptorRegistrationProvider>(registry));
            return builder;
        }
    }
}
