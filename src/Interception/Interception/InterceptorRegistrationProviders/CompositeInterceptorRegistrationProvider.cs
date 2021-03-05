using System;
using System.Collections.Generic;
using System.Linq;

namespace Dora.Interception
{
    internal sealed class CompositeInterceptorRegistrationProvider : IInterceptorRegistrationProvider
    {
        private readonly IEnumerable<IInterceptorRegistrationProvider> _providers;

        public CompositeInterceptorRegistrationProvider(IEnumerable<IInterceptorRegistrationProvider> providers)
        {
            _providers = providers;
        }

        public IEnumerable<InterceptorRegistration> GetRegistrations(Type serviceType)
        {
            return _providers.SelectMany(it => it.GetRegistrations(serviceType));
        }
     }
}
