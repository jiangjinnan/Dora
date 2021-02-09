using System;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Builder for interception based service registration.
    /// </summary>
    public sealed class InterceptionBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InterceptionBuilder"/> class.
        /// </summary>
        /// <param name="services">The service registrations.</param>
        public InterceptionBuilder(IServiceCollection services)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));
        }

        /// <summary>
        /// Gets the service registrations.
        /// </summary>
        /// <value>
        /// The service registrations.
        /// </value>
        public IServiceCollection Services { get; }
    }
}
