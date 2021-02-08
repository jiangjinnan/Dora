using System;

namespace Dora.Interception
{
    public class ServiceProviderAccessor : IServiceProviderAccessor
    {
        private readonly IServiceProvider _serviceProvider;
        public ServiceProviderAccessor(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        public IServiceProvider ServiceProvider => _serviceProvider;
    }
}
