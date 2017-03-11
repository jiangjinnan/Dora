using Dora;
using Dora.Interception;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Defines extension methods specific to <see cref="IServiceProvider"/>.
    /// </summary>
    public static class ServiceProviderExtensions
    {
        /// <summary>
        /// Convert to the service provider from which the services are provided can be intercepted.
        /// </summary>
        /// <param name="serviceProvider">The service provider to convert.</param>
        /// <returns>The service provider from which the services are provided can be intercepted.</returns>
        /// <exception cref="ArgumentNullException">The argument <paramref name="serviceProvider"/> is null.</exception>
        public static IServiceProvider ToInterceptable(this IServiceProvider serviceProvider)
        {
            Guard.ArgumentNotNull(serviceProvider, "serviceProvider");
            return new InterceptableServiceProvider(serviceProvider);
        }
    }
}
