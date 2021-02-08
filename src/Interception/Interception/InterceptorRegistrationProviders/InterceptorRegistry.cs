using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dora.Interception
{
    public class InterceptorRegistry : IInterceptorRegistry
    {
        private readonly Dictionary<Type, List<InterceptorRegistration>> _registrations = new Dictionary<Type, List<InterceptorRegistration>>();
        public IInterceptorRegistry Register<TInterceptor>(Action<IInterceptorAssigner> assignment, params object[] arguments)
        {
            object CreateInterceptor(IServiceProvider serviceProvider) => ActivatorUtilities.CreateInstance<TInterceptor>(serviceProvider, arguments);
            var assigner = new InterceptorAssigner();
            assignment(assigner);
            var typedMethods = assigner.GetAssignedMethods();
            foreach (var kv in typedMethods)
            {
                var type = kv.Key;
                var list = _registrations.TryGetValue(type, out var value)
                    ? value
                    : _registrations[type] = new List<InterceptorRegistration>();
                list.AddRange(kv.Value.Select(it => new InterceptorRegistration(CreateInterceptor, it.Key, it.Value)));
            }
            return this;
        }

        public IEnumerable<InterceptorRegistration> GetRegistrations(Type type)
        {
            return _registrations.TryGetValue(type, out var value)
                ? value
                : Enumerable.Empty<InterceptorRegistration>();
        }
    }
}
