using Dora.Interception.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Dora.Interception
{
    internal class CompositeInterceptorProviderResolver : IInterceptorProviderResolver
    {
        private readonly IInterceptorProviderResolver[]  _providerResolvers;

        public CompositeInterceptorProviderResolver(IEnumerable<IInterceptorProviderResolver> providerResolvers)
        {
            _providerResolvers = Guard.ArgumentNotNullOrEmpty(providerResolvers, nameof(providerResolvers)).ToArray();
        }

        public IInterceptorProvider[] GetInterceptorProvidersForType(Type type, out ISet<Type> excludedInterceptorProviders)
        {
            Guard.ArgumentNotNull(type, nameof(type));
            if (type.IsGenericType)
            {
                type = type.GetGenericTypeDefinition();
            }

            var providers = new List<IInterceptorProvider>();
            var execluded = new List<Type>();

            for (int index = 0; index < _providerResolvers.Length; index++)
            {
                var resolver = _providerResolvers[index];
                providers.AddRange(resolver.GetInterceptorProvidersForType(type, out var excludedProviders));
                execluded.AddRange(excludedProviders);
            }

            excludedInterceptorProviders = new HashSet<Type>(execluded);
            return providers.ToArray();
        }

        public IInterceptorProvider[] GetInterceptorProvidersForMethod(Type targetType, MethodInfo method, out ISet<Type> excludedInterceptorProviders)
        {
            Guard.ArgumentNotNull(method, nameof(method));

            var providers = new List<IInterceptorProvider>();
            var execluded = new List<Type>();

            for (int index = 0; index < _providerResolvers.Length; index++)
            {
                var resolver = _providerResolvers[index];
                providers.AddRange(resolver.GetInterceptorProvidersForMethod(targetType, method, out var excludedProviders));
                execluded.AddRange(excludedProviders);
            }

            excludedInterceptorProviders = new HashSet<Type>(execluded);
            return providers.ToArray();
        } 
       
        public IInterceptorProvider[] GetInterceptorProvidersForProperty(Type targetType, PropertyInfo property, PropertyMethod propertyMethod, out ISet<Type> excludedInterceptorProviders)
        {
            Guard.ArgumentNotNull(property, nameof(property));
            if (propertyMethod == PropertyMethod.Get && property.GetMethod == null)
            {
                throw new ArgumentException(Resources.PropertyHasNoGetMethod.Fill(property.Name, property.DeclaringType.AssemblyQualifiedName), nameof(propertyMethod));
            }
            if (propertyMethod == PropertyMethod.Set && property.SetMethod == null)
            {
                throw new ArgumentException(Resources.PropertyHasNoSetMethod.Fill(property.Name, property.DeclaringType.AssemblyQualifiedName), nameof(propertyMethod));
            }

            var providers = new List<IInterceptorProvider>();
            var execluded = new List<Type>();

            for (int index = 0; index < _providerResolvers.Length; index++)
            {
                var resolver = _providerResolvers[index];
                providers.AddRange(resolver.GetInterceptorProvidersForProperty(targetType, property, propertyMethod, out var excludedProviders));
                execluded.AddRange(excludedProviders);
            }

            excludedInterceptorProviders = new HashSet<Type>(execluded);
            return providers.ToArray();
        }

        public bool? WillIntercept(Type type)
        {
            Guard.ArgumentNotNull(type, nameof(type));
            for (int index = _providerResolvers.Length - 1; index >= 0; index--)
            {
                var result = _providerResolvers[index].WillIntercept(type);
                if (result == null)
                {
                    continue;
                }
                return result.Value;
            }
            return null;
        }

        public bool? WillIntercept(Type targetType, MethodInfo method)
        {
            Guard.ArgumentNotNull(targetType, nameof(targetType));
            Guard.ArgumentNotNull(method, nameof(method));
            for (int index = _providerResolvers.Length - 1; index >= 0; index--)
            {
                var result = _providerResolvers[index].WillIntercept(targetType, method);
                if (result == null)
                {
                    continue;
                }
                return result.Value;
            }
            return null;
        }

        public bool? WillIntercept(Type targetType, PropertyInfo property)
        {
            Guard.ArgumentNotNull(targetType, nameof(targetType));
            Guard.ArgumentNotNull(property, nameof(property));
            for (int index = _providerResolvers.Length - 1; index >= 0; index--)
            {
                var result = _providerResolvers[index].WillIntercept(targetType, property);
                if (result == null)
                {
                    continue;
                }
                return result.Value;
            }
            return null;
        }
    }
}
