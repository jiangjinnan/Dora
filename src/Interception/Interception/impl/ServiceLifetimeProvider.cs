using Microsoft.Extensions.DependencyInjection;

namespace Dora.Interception
{
    internal class ServiceLifetimeProvider : IServiceLifetimeProvider
    {
        private readonly IServiceCollection _services;
        public ServiceLifetimeProvider(IServiceCollection services)=> _services = services ?? throw new ArgumentNullException(nameof(services));
        public ServiceLifetime? GetLifetime(Type serviceType) => _services.LastOrDefault(it => it.ServiceType == serviceType)?.Lifetime;
    }
}
