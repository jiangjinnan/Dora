using Dora;
using Dora.ExceptionHandling;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Defines exension method to register <see cref="ExceptionManagerBuilder"/> and dependent services.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Register the <see cref="ExceptionManager"/> based service with exception policies.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to which the service is registered.</param>
        /// <param name="configure">A <see cref="Action{IExceptionManagerBuilder}"/> to set exception policy.</param>
        /// <returns>The current <see cref="IServiceCollection"/> with the <see cref="ExceptionManager"/> based service.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="services"/> is null.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="configure"/> is null.</exception>
        public static IServiceCollection AddExceptionHandling(this IServiceCollection services, Action<IExceptionManagerBuilder> configure)
        {
            Guard.ArgumentNotNull(services, nameof(services));
            Guard.ArgumentNotNull(configure, nameof(configure));
            return services.AddSingleton<ExceptionManager>(serviceProvider =>
            {
                ExceptionManagerBuilder builder = new ExceptionManagerBuilder(serviceProvider);
                configure(builder);
                return builder.Build();
            });
        }
    }
}