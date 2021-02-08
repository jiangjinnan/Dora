using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.Interception
{
    public class InterceptionContainer
    {
        private readonly IServiceCollection _services;

        public InterceptionContainer(IServiceCollection services)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
        }

        public IServiceProvider BuildServiceProvider()
        {
            var serviceProvider = _services.BuildServiceProvider();
            var registrationProvider = new CompositeInterceptorRegistrationProvider(serviceProvider.GetRequiredService<IEnumerable< IInterceptorRegistrationProvider>>());
            var generator = serviceProvider.GetRequiredService<IInterceptableProxyGenerator>();
            var newServices = new ServiceCollection();
            foreach (var service in _services)
            {
                var list = (IList<ServiceDescriptor>)newServices;
                if (!WillIntercept(service))
                {
                    list.Add(service);
                }
                else
                {
                   list.Add(new ServiceDescriptor(service.ServiceType, generator.Generate(service.ServiceType, service.ImplementationType), service.Lifetime));
                }
            }
            return newServices.BuildServiceProvider();

            bool WillIntercept(ServiceDescriptor service)
            {
                if (service.ImplementationInstance != null || service.ImplementationFactory != null || service.ImplementationType == null)
                {
                    return false;
                }

                return registrationProvider.WillIntercept(service.ImplementationType);
            }
        }
    }
}
