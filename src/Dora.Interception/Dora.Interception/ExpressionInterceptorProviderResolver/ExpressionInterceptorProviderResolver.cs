using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;

namespace Dora.Interception
{
    internal class ExpressionInterceptorProviderResolver : IInterceptorProviderResolver
    {
        private InterceptorProviderRegistration[] _registrations;

        public ExpressionInterceptorProviderResolver(IEnumerable<InterceptorProviderRegistration> registrations)
        {
            _registrations = Guard.ArgumentNotNullOrEmpty(registrations, nameof(registrations)).ToArray();
        }
        public IInterceptorProvider[] GetInterceptorProvidersForMethod(MethodInfo method)
        {   
            return (from it in _registrations
                    where it.TargetRegistrations.Any(it2 => it2.IncludedMethods.Contains(method))
                    select it.InterceptorProviderFactory())
                    .ToArray();
        }

        public IInterceptorProvider[] GetInterceptorProvidersForProperty(PropertyInfo property, PropertyMethod propertyMethod)
        {
            return (from it in _registrations
                    where it.TargetRegistrations.Any(it2 => it2.IncludedProperties.Any(it3=>it3.Key == property && (it3.Value & propertyMethod) == it3.Value))
                    select it.InterceptorProviderFactory())
                    .ToArray();
        }

        public IInterceptorProvider[] GetInterceptorProvidersForType(Type type)
        {
            return (from it in _registrations
                    where it.TargetRegistrations.Any(it2 => it2.TargetType == type && it2.IncludedAllMembers == true && it2.ExludedAllMembers != true)
                    select it.InterceptorProviderFactory())
                    .ToArray();
        }

        public bool? WillIntercept(Type type) => null;

        public bool? WillIntercept(MethodInfo method) => null;

        public bool? WillIntercept(PropertyInfo property) => null;
    }
}
