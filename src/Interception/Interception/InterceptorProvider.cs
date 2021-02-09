using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Dora.Interception
{
    public class InterceptorProvider : IInterceptorProvider
    {
        private readonly ConcurrentDictionary<Type, IEnumerable<InterceptorRegistration>> _registrations;
        private readonly ConcurrentDictionary<MethodInfo, IInterceptor> _interceptors;
        private readonly IInterceptorRegistrationProvider[] _registrationProviders;
        private readonly IInterceptorBuilder _interceptorBuilder;

        public InterceptorProvider(IEnumerable<IInterceptorRegistrationProvider> registrationProviders, IInterceptorBuilder interceptorBuilder)
        {
            _registrationProviders = (registrationProviders ?? throw new ArgumentNullException(nameof(registrationProviders))).ToArray();
            _interceptorBuilder = interceptorBuilder ?? throw new ArgumentNullException(nameof(interceptorBuilder));
            _registrations = new ConcurrentDictionary<Type, IEnumerable<InterceptorRegistration>>();
            _interceptors = new ConcurrentDictionary<MethodInfo, IInterceptor>();
        }

        public IInterceptor GetInterceptor(MethodInfo method) => _interceptors.GetOrAdd(method, CreateInterceptor);
        private IInterceptor CreateInterceptor(MethodInfo method)
        {
            var registrations = _registrations
                .GetOrAdd(method.DeclaringType, GetRegistrations)
                .Where(it => it.TargetMethod == method)
                .OrderBy(it=>it.Order);
            return _interceptorBuilder.Build(registrations);

            IEnumerable<InterceptorRegistration> GetRegistrations(Type type)
                => _registrationProviders.SelectMany(it => it.GetRegistrations(type)).ToArray();
        }
    }
}
