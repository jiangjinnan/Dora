using Dora;
using Dora.Interception;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Microsoft.Extensions.Hosting
{
    /// <summary>
    /// Defines <see cref="IHostBuilder"/> specific extension methods.
    /// </summary>
    public static class HostBuilderExtensions
    {
        /// <summary>
        /// Registers <see cref="InterceptableServiceProviderFactory"/> into hosting system.
        /// </summary>
        /// <param name="builder">The <see cref="IHostBuilder"/> to build <see cref="IHost"/>.</param>
        /// <param name="options">The default <see cref="IServiceProvider"/> based options.</param>
        /// <param name="configure">The <see cref="Action{InterceptionBuilder}"/> to perform further interception based service registrations.</param>
        /// <returns>The current <see cref="IHostBuilder"/>.</returns>
        public static IHostBuilder UseInterceptableServiceProvider(
            this IHostBuilder builder,
            ServiceProviderOptions options = null,
            Action<InterceptionBuilder> configure = null)
        {
            Guard.ArgumentNotNull(builder, nameof(builder));
            return builder.UseServiceProviderFactory(new InterceptableServiceProviderFactory(options, configure));
        }
    }
}
