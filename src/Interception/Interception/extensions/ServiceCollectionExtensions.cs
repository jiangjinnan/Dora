using Dora;
using Dora.Interception;
using Dora.Interception.CodeGeneration;
using Dora.Interception.Expressions;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Defines extension methods to register interception based services.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the interception based services.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> containg all service registrations.</param>
        /// <param name="setup">The <see cref="Action{InterceptionBuilder}"/> used to advanced service registering.</param>
        /// <returns>The passed <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddInterception(this IServiceCollection services, Action<InterceptionBuilder>? setup = null)
        {
            Guard.ArgumentNotNull(services);
            services.AddOptions();
            services.AddLogging();
            services.TryAddSingleton<IApplicationServicesAccessor, ApplicationServicesAccessor>();
            services.TryAddSingleton<IInvocationServiceScopeFactory, InvocationServiceScopeFactory>();
            services.TryAddSingleton<IMethodInvokerBuilder, DefaultMethodInvokerBuilder>();
            services.TryAddSingleton<IConventionalInterceptorFactory, ConventionalInterceptorFactory>();
            services.TryAddEnumerable(ServiceDescriptor.Singleton<ICodeGenerator, InterfaceProxyGenerator>());
            services.TryAddEnumerable(ServiceDescriptor.Singleton<ICodeGenerator, VirtualMethodProxyGenerator>());
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IInterceptorProvider, DataAnnotationInterceptorProvider>());
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IInterceptorProvider, ExpressionInterceptorProvider>());
            setup?.Invoke(new InterceptionBuilder(services));

            return services;
        }

        /// <summary>
        /// Builds the interceptable <see cref="IServiceProvider"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> containg all service registrations.</param>
        /// <param name="setup">The setup.</param>
        /// <returns>The built interceptable <see cref="IServiceProvider"/>.</returns>
        public static IServiceProvider BuildInterceptableServiceProvider(this IServiceCollection services, Action<InterceptionBuilder>? setup = null)
            => BuildInterceptableServiceProvider(services, new ServiceProviderOptions(), setup);

        /// <summary>
        /// Builds the interceptable <see cref="IServiceProvider"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> containg all service registrations.</param>
        /// <param name="setup">The setup.</param>
        /// <param name="serviceProviderOptions">The options for configuring various behaviors of the default <see cref="IServiceProvider"/> implementation.</param>
        /// <returns>The built interceptable <see cref="IServiceProvider"/>.</returns>
        public static IServiceProvider BuildInterceptableServiceProvider(this IServiceCollection services, ServiceProviderOptions serviceProviderOptions, Action<InterceptionBuilder>? setup = null)
        {
            Guard.ArgumentNotNull(services);
            Guard.ArgumentNotNull(serviceProviderOptions);
            var factgory = new InterceptableServiceProviderFactory(serviceProviderOptions, setup);
            var builder = factgory.CreateBuilder(services);
            return builder.CreateServiceProvider();
        }
    }
}
