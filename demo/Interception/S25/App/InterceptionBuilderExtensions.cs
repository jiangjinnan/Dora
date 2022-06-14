using Dora.Interception;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace App
{
    public static class InterceptionBuilderExtensions
    {
        public static InterceptionBuilder RegisterInterceptors(this InterceptionBuilder builder, Action<ConditionalInterceptorProviderOptions> setup)
        {
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IInterceptorProvider, ConditionalInterceptorProvider>());
            builder.Services.Configure(setup);
            return builder;
        }
    }
}
