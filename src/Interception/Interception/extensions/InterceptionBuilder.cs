namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// A builder used to register interception based services.
    /// </summary>
    public sealed class InterceptionBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InterceptionBuilder"/> class.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> containing all service registrations.</param>
        public InterceptionBuilder(IServiceCollection services)=> Services = services ?? throw new ArgumentNullException(nameof(services));

        /// <summary>
        /// Gets <see cref="IServiceCollection"/> containing all service registrations.
        /// </summary>
        /// <value>
        /// The <see cref="IServiceCollection"/> containing all service registrations.
        /// </value>
        public IServiceCollection Services { get; }
    }
}
