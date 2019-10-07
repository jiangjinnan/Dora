using Microsoft.Extensions.DependencyInjection;
using System;

namespace Dora.Interception
{
    /// <summary>
    /// Represents the custom <see cref="IServiceProviderFactory{TContainerBuilder}"/> for interception extensions.
    /// </summary>
    public sealed class InterceptableServiceProviderFactory : IServiceProviderFactory<IServiceCollection>
    {
        #region Fields
        private readonly Action<InterceptionBuilder> _configure;
        private readonly ServiceProviderOptions _options;
        #endregion

        /// <summary>
        /// Create a new <see cref="InterceptableServiceProviderFactory"/>.
        /// </summary>
        /// <param name="options">Options for configuring various behaviors of the default <see cref="IServiceProvider"/> implementation.</param>
        /// <param name="configure">The <see cref="Action{InterceptionBuilder}"/> used to perform more service registrations.</param>
        public InterceptableServiceProviderFactory(ServiceProviderOptions options, Action<InterceptionBuilder> configure)
        {
            _configure = configure;
            _options = options;
        }

        /// <summary>
        /// Creates a container builder from an Microsoft.Extensions.DependencyInjection.IServiceCollection.
        /// </summary>
        /// <param name="services">The contract for a collection of service descriptors.</param>
        /// <returns>The <see cref="IServiceCollection"/> with interception based service registrations.</returns>
        public IServiceCollection CreateBuilder(IServiceCollection services)
        {
            Guard.ArgumentNotNull(services, nameof(services));
            services.TryAddInterception(_configure);

            var provider = services.BuildServiceProvider();
            var factoryCache = provider.GetRequiredService<IInterceptableProxyFactoryCache>();
            var resolver = provider.GetRequiredService<IInterceptorResolver>();
            var codeGeneratorFactory = provider.GetRequiredService<ICodeGeneratorFactory>();

            IServiceCollection newServices = new ServiceCollection();
            foreach (var service in services)
            {
                foreach (var newService in new ServiceDescriptorConverter(service, resolver, factoryCache, codeGeneratorFactory).AsServiceDescriptors())
                {
                    newServices.Add(newService);
                }
            }

            return newServices;
        }

        /// <summary>
        /// Creates an <see cref="IServiceProvider"/> from the container builder.
        /// </summary>
        /// <param name="containerBuilder">The <see cref="IServiceCollection"/> with interception based service registrations.</param>
        /// <returns>The created <see cref="IServiceProvider"/>.</returns>
        public IServiceProvider CreateServiceProvider(IServiceCollection containerBuilder)
            => _options == null
            ? containerBuilder.BuildServiceProvider()
            : containerBuilder.BuildServiceProvider(_options);
    }
}
