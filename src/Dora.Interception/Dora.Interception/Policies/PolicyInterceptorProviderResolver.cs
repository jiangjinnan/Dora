using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;

namespace Dora.Interception.Policies
{
    internal class PolicyInterceptorProviderResolver : IInterceptorProviderResolver
    {
        private readonly InterceptionPolicy _policy;
        private readonly IInterceptorProvider[] _empty;

        public PolicyInterceptorProviderResolver(InterceptionPolicy policy)
        {
            _policy = Guard.ArgumentNotNull(policy, nameof(policy));
            _empty = new IInterceptorProvider[0];
        }
        public IInterceptorProvider[] GetInterceptorProvidersForMethod(Type targetType, MethodInfo method)
        {
            bool Filter(InterceptorProviderPolicy registration)
            {
                var target = registration.TargetPolicies
                    .Where(it => it.TargetType == targetType)
                    .SingleOrDefault();

                //is explicitly excluded.
                if (target == null || target.ExludedMethods.Contains(method.MetadataToken))
                {
                    return false;
                }

                //is explicitly included.
                if (target.IncludedMethods.Contains(method.MetadataToken))
                {
                    return true;
                }

                //Incldue all members
                if (target.IncludeAllMembers == true)
                {
                    return true;
                }

                return false;
            }

            return (from it in _policy
                    where Filter(it)
                    select it.InterceptorProviderFactory())
                    .ToArray();
        }       
        public IInterceptorProvider[] GetInterceptorProvidersForProperty(Type targetType, PropertyInfo property, PropertyMethod propertyMethod)
        {
            bool Filter(InterceptorProviderPolicy registration)
            {
                var target = registration.TargetPolicies.Where(it => it.TargetType == targetType).FirstOrDefault();

                //is explicitly excluded.
                if (target == null || (target.ExcludedProperties.TryGetValue(property.MetadataToken, out var methodType) && (methodType == propertyMethod || methodType == PropertyMethod.Both)))
                {
                    return false;
                }

                //is explicitly included.
                if (target.IncludedProperties.TryGetValue(property.MetadataToken, out methodType) && (methodType == propertyMethod || methodType == PropertyMethod.Both))
                {
                    return true;
                }

                //Incldue all members
                if (target.IncludeAllMembers == true)
                {
                    return true;
                }

                return false;
            }

            return (from it in _policy
                    where Filter(it)
                    select it.InterceptorProviderFactory())
                    .ToArray();
        }    
        public IInterceptorProvider[] GetInterceptorProvidersForType(Type type) => _empty; 
        public bool? WillIntercept(Type type)
        {
            return _policy.Any(it1 => it1.TargetPolicies.Any(it2 => it2.TargetType == type));
        }
        //public bool? WillIntercept(Type targetType, MethodInfo method) => null;
        //public bool? WillIntercept(Type targetType, PropertyInfo property) => null;
    }
}
