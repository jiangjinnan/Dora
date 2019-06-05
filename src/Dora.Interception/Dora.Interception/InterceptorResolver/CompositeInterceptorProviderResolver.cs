using Dora.Interception.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Dora.Interception
{
    internal class CompositeInterceptorProviderResolver : IInterceptorProviderResolver
    {
        private IInterceptorProviderResolver[]  _providerResolvers;

        public CompositeInterceptorProviderResolver(IEnumerable<IInterceptorProviderResolver> providerResolvers)
        {
            _providerResolvers = Guard.ArgumentNotNullOrEmpty(providerResolvers, nameof(providerResolvers)).ToArray();
        }

        public IInterceptorProvider[] GetInterceptorProvidersForType(Type type)
        {
            Guard.ArgumentNotNull(type, nameof(type));
            if (type.IsGenericType)
            {
                type = type.GetGenericTypeDefinition();
            }
            return _providerResolvers
                .SelectMany(it => it.GetInterceptorProvidersForType(type))
                .ToArray();
        }

        public IInterceptorProvider[] GetInterceptorProvidersForMethod(Type targetType, MethodInfo method)
        {
            Guard.ArgumentNotNull(method, nameof(method));
            return _providerResolvers
                .SelectMany(it => it.GetInterceptorProvidersForMethod(targetType,method))
                .ToArray();
        } 
       
        public IInterceptorProvider[] GetInterceptorProvidersForProperty(Type targetType, PropertyInfo property, PropertyMethod propertyMethod)
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
            return _providerResolvers
                .SelectMany(it => it.GetInterceptorProvidersForProperty(targetType,property, propertyMethod))
                .ToArray();
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
    }
}
