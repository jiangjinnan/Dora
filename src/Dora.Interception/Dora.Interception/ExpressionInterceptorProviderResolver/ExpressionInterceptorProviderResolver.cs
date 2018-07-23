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
        private IInterceptorProvider[] _empty;

        public ExpressionInterceptorProviderResolver(IEnumerable<InterceptorProviderRegistration> registrations)
        {
            _registrations = Guard.ArgumentNotNullOrEmpty(registrations, nameof(registrations)).ToArray();
            _empty = new IInterceptorProvider[0];
        }
        public IInterceptorProvider[] GetInterceptorProvidersForMethod(Type targetType, MethodInfo method)
        {
            Func<InterceptorProviderRegistration, bool> filter = registration =>
            {
                var target = registration.TargetRegistrations.Where(it => it.TargetType == targetType).FirstOrDefault();
                if (target == null || target.ExludedMethods.Contains(method.MetadataToken))
                {
                    return false; 
                }

                if (target.IncludedMethods.Contains(method.MetadataToken))
                {
                    return true;
                }

                if (target.ExludedAllMembers == true)
                {
                    return false;
                }

                if (target.IncludedAllMembers == true)
                {
                    return true;
                }

                return false;
            };

            return (from it in _registrations
                    where filter(it)
                    select it.InterceptorProviderFactory())
                    .ToArray();
        }

        public IInterceptorProvider[] GetInterceptorProvidersForProperty(Type targetType, PropertyInfo property, PropertyMethod propertyMethod)
        {
            Func<InterceptorProviderRegistration, bool> filter = registration =>
            {
                var target = registration.TargetRegistrations.Where(it => it.TargetType == targetType).FirstOrDefault();
                if (target == null || (target.ExludedProperties.TryGetValue(property.MetadataToken, out var methodType) && (methodType == propertyMethod || methodType == PropertyMethod.Both)))
                {
                    return false;
                }

                if (target.IncludedProperties.TryGetValue(property.MetadataToken, out methodType) && (methodType == propertyMethod || methodType == PropertyMethod.Both))
                {
                    return true;
                }

                if (target.ExludedAllMembers == true)
                {
                    return false;
                }

                if (target.IncludedAllMembers == true)
                {
                    return true;
                }

                return false;
            };

            return (from it in _registrations
                    where filter(it)
                    select it.InterceptorProviderFactory())
                    .ToArray();
        }

        public IInterceptorProvider[] GetInterceptorProvidersForType(Type type) => _empty;

        public bool? WillIntercept(Type type)
        {
            return _registrations.Any(it1 => it1.TargetRegistrations.Any(it2 => it2.TargetType == type));
        }
        public bool? WillIntercept(Type targetType, MethodInfo method) => null;
        public bool? WillIntercept(Type targetType, PropertyInfo property) => null;
    }
}
