using Dora;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// A builder which helps register interception based services.
    /// </summary>
    public class InterceptionBuilder
    {
        /// <summary>
        /// Gets the service collection in which the interception based services are added.
        /// </summary>
        public IServiceCollection Services { get; }

        /// <summary>
        /// Create a new <see cref="InterceptionBuilder"/>.
        /// </summary>
        /// <param name="services">The service collection in which the interception based services are added.</param>
        /// <exception cref="ArgumentNullException">The argument <paramref name="services"/> is null.</exception>
        public InterceptionBuilder(IServiceCollection services)
        {
            Guard.ArgumentNotNull(services, nameof(services));
            this.Services = services;
        }
    }
}
