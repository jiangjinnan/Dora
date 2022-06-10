using Microsoft.Extensions.DependencyInjection;

namespace Dora.Interception
{
    /// <summary>
    /// Provider to get service life time.
    /// </summary>
    public interface IServiceLifetimeProvider
    {
        /// <summary>
        /// Gets the lifetime of specified type based service registration.
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        /// <returns>The life time of the conrresponding service registration.</returns>
        ServiceLifetime? GetLifetime(Type serviceType);
    }
}
