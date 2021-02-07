using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Dora.Interception
{
    public class InterceptorProvider : IInterceptorProvider
    {
        private readonly Dictionary<MethodInfo, IInterceptor> _interceptors;
        public InterceptorProvider(IInterceptorBuilder builder, IInterceptorRegistrationProvider registrationProvider)
        {
            builder = builder ?? throw new ArgumentNullException(nameof(builder));
            registrationProvider = registrationProvider ?? throw new ArgumentNullException(nameof(registrationProvider));

            var registrations = registrationProvider.Registrations;
            _interceptors = registrations
                .GroupBy(it => it.Target)
                .ToDictionary(it => it.Key, it => it.OrderBy(reg => reg.Order))
                .ToDictionary(it => it.Key, it => builder.Build(it.Value));
        }
        public IInterceptor GetInterceptor(MethodInfo method) => _interceptors.TryGetValue(method, out var v) ? v : null;
    }
}
