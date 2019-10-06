using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Dora.Interception
{
    internal class AttributeInterceptorProviderResolver : IInterceptorProviderResolver
    {
        public static IInterceptorProvider[] Empty { get; } = new IInterceptorProvider[0];

        public IInterceptorProvider[] GetInterceptorProvidersForType(Type type, out ISet<Type> excludedInterceptorProviders)
        {
            Guard.ArgumentNotNull(nameof(type), nameof(type));
            var excludedTypes = CustomAttributeAccessor.GetCustomAttribute<NonInterceptableAttribute>(type)?.InterceptorProviderTypes;
            excludedInterceptorProviders = excludedTypes?.Length > 0
                ? new HashSet<Type>(excludedTypes)
                : new HashSet<Type>();
            return CustomAttributeAccessor.GetCustomAttributes<IInterceptorProvider>(type).Reverse().ToArray();
        }

        public IInterceptorProvider[] GetInterceptorProvidersForMethod(Type targetType, MethodInfo method, out ISet<Type> excludedInterceptorProviders)
        {
            Guard.ArgumentNotNull(nameof(method), nameof(method));
            var excludedTypes = CustomAttributeAccessor.GetCustomAttribute<NonInterceptableAttribute>(method)?.InterceptorProviderTypes;
            excludedInterceptorProviders = excludedTypes?.Length > 0
                ? new HashSet<Type>(excludedTypes)
                : new HashSet<Type>();
            return CustomAttributeAccessor.GetCustomAttributes<IInterceptorProvider>(method).Reverse().ToArray();
        }

        public IInterceptorProvider[] GetInterceptorProvidersForProperty(Type targetType, PropertyInfo property, PropertyMethod propertyMethod, out ISet<Type> excludedInterceptorProviders)
        {
            Guard.ArgumentNotNull(nameof(property), nameof(property));
            var method = propertyMethod == PropertyMethod.Get ? property.GetMethod : property.SetMethod;

            var list = new List<Type>();
            var excludedTypes = CustomAttributeAccessor.GetCustomAttribute<NonInterceptableAttribute>(property)?.InterceptorProviderTypes;
            if (excludedTypes?.Length > 0)
            {
                list.AddRange(excludedTypes);
            }
            excludedTypes = CustomAttributeAccessor.GetCustomAttribute<NonInterceptableAttribute>(method)?.InterceptorProviderTypes;
            if (excludedTypes?.Length > 0)
            {
                list.AddRange(excludedTypes);
            }
            excludedInterceptorProviders = new HashSet<Type>(list);
           
            var providers = CustomAttributeAccessor.GetCustomAttributes<IInterceptorProvider>(property);
            providers.Concat(CustomAttributeAccessor.GetCustomAttributes<IInterceptorProvider>(method));
            return providers.ToArray();
        }

        public bool? WillIntercept(Type type)
        {
            Guard.ArgumentNotNull(nameof(type), nameof(type));
            var attributes = CustomAttributeAccessor.GetCustomAttributes<NonInterceptableAttribute>(type);
            if (attributes.Any(it => it.InterceptorProviderTypes.Length == 0))
            {
                return false;
            }
            return null;
        }

        public bool? WillIntercept(Type targetType, MethodInfo method)
        {
            Guard.ArgumentNotNull(nameof(method), nameof(method));
            var attributes = CustomAttributeAccessor.GetCustomAttributes<NonInterceptableAttribute>(method, true);
            if (attributes.Any(it => it.InterceptorProviderTypes.Length == 0))
            {
                return false;
            }
            return null;
        }

        public bool? WillIntercept(Type targetType, PropertyInfo property)
        {
            Guard.ArgumentNotNull(nameof(property), nameof(property));
            var attributes = CustomAttributeAccessor.GetCustomAttributes<NonInterceptableAttribute>(property, true);
            if (attributes.Any(it => it.InterceptorProviderTypes.Length == 0))
            {
                return false;
            }
            return null;
        }
    }
}