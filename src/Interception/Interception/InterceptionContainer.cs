using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.Interception
{
    public sealed class InterceptionContainer
    {
        private readonly IServiceCollection _services;

        public InterceptionContainer(IServiceCollection services)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
        }

        public IServiceProvider BuildServiceProvider()
        {
            var serviceProvider = _services.BuildServiceProvider();
            var registrationProvider = new CompositeInterceptorRegistrationProvider(serviceProvider.GetRequiredService<IEnumerable<IInterceptorRegistrationProvider>>());
            var generators = serviceProvider.GetServices<IInterceptableProxyGenerator>();
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
                    var intercepted = false;
                    foreach (var generator in generators)
                    {
                        var proxyType = generator.Generate(service.ServiceType, service.ImplementationType);
                        if (null != proxyType)
                        {
                            intercepted = true;
                            list.Add(new ServiceDescriptor(service.ServiceType, proxyType, service.Lifetime));
                            continue;
                        }
                    }
                    if (!intercepted)
                    {
                        list.Add(service);
                    }
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
