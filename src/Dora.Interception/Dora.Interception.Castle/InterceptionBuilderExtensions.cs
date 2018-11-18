using Dora;
using Dora.Interception;
using Dora.Interception.Castle;
using System;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Defines some extension methods specific to <see cref="InterceptionBuilder"/>.
    /// </summary>
    public static class InterceptionBuilderExtensions
    {
        /// <summary>
        /// Register <see cref="DynamicProxyFactory"/>.
        /// </summary>
        /// <param name="builder">The <see cref="InterceptionBuilder"/> to which the <see cref="DynamicProxyFactory"/> is registered.</param>
        /// <returns>The <see cref="InterceptionBuilder"/> with the service registration of <see cref="DynamicProxyFactory"/> is registered.</returns>
        ///<exception cref="ArgumentNullException">The argument <paramref name="builder"/> is null.</exception>
        public static InterceptionBuilder SetCastleDynamicProxy(this InterceptionBuilder builder)
        {
            Guard.ArgumentNotNull(builder, nameof(builder));
            builder.Services.Replace(ServiceDescriptor.Singleton<IInterceptingProxyFactory, DynamicProxyFactory>());
            return builder;
        }
    }
}
