using Dora.Interception;
using Dora.Interception.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.Hosting
{
    /// <summary>
    /// Defines <see cref="IHostBuilder"/> specific extension methods.
    /// </summary>
    public static class HostBuilderExtensions
    {
        /// <summary>
        /// Apply interception machanism to dependency injection framework.
        /// </summary>
        /// <param name="hostBuilder">The <see cref="IHostBuilder"/> to build service host.</param>
        /// <param name="setup">The <see cref="Action{InterceptionBuilder}"/> to perform advanced service registrations.</param>
        /// <returns>The current <see cref="IHostBuilder"/>.</returns>
        public static IHostBuilder UseInterception(this IHostBuilder hostBuilder, Action<InterceptionBuilder>? setup = null)
            => UseInterception(hostBuilder, new ServiceProviderOptions(), setup);


        /// <summary>
        /// Apply interception machanism to dependency injection framework.
        /// </summary>
        /// <param name="hostBuilder">The <see cref="IHostBuilder"/> to build service host.</param>
        /// <param name="serviceProviderOptions">The options for configuring various behaviors of the default <see cref="IServiceProvider"/> implementation.</param>
        /// <param name="setup">The <see cref="Action{InterceptionBuilder}"/> to perform advanced service registrations.</param>
        /// <returns>The current <see cref="IHostBuilder"/>.</returns>
        public static IHostBuilder UseInterception(this IHostBuilder hostBuilder, ServiceProviderOptions serviceProviderOptions, Action<InterceptionBuilder>? setup = null)
        {
            if (hostBuilder == null) throw new ArgumentNullException(nameof(hostBuilder));
            if (serviceProviderOptions == null) throw new ArgumentNullException(nameof(serviceProviderOptions));
            Action<InterceptionBuilder>  configure = builder => {
                builder.Services.Replace(ServiceDescriptor.Singleton<IInvocationServiceScopeFactory, RequestServiceScopeFactory>());
                setup?.Invoke(builder);
            };
            return hostBuilder.UseServiceProviderFactory(new InterceptionServiceProviderFactory(serviceProviderOptions?? new ServiceProviderOptions(), configure));
        }

        private class InterceptionServiceProviderFactory : IServiceProviderFactory<InterceptableContainerBuilder>
        {
            private readonly Action<InterceptionBuilder>? _setup;
            private readonly ServiceProviderOptions _providerOptions;

            public InterceptionServiceProviderFactory(ServiceProviderOptions providerOptions, Action<InterceptionBuilder>? setup)
            {
                _setup = setup;
                _providerOptions = providerOptions;
            }

            public InterceptableContainerBuilder CreateBuilder(IServiceCollection services) => new(services, _providerOptions, _setup);
            public IServiceProvider CreateServiceProvider(InterceptableContainerBuilder containerBuilder) => containerBuilder.CreateServiceProvider();
        }
    }
}
