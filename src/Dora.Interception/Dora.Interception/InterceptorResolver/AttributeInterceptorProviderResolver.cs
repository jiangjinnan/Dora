using System;
using System.Linq;
using System.Reflection;

namespace Dora.Interception
{
    internal class AttributeInterceptorProviderResolver : IInterceptorProviderResolver
    {
        public static IInterceptorProvider[] Empty { get; } = new IInterceptorProvider[0];

        public IInterceptorProvider[] GetInterceptorProvidersForType(Type type)
        {
            Guard.ArgumentNotNull(nameof(type), nameof(type));            
            return CustomAttributeAccessor.GetCustomAttributes<IInterceptorProvider>(type)
                .Reverse()
                .ToArray();
        }

        public IInterceptorProvider[] GetInterceptorProvidersForMethod(Type targetType, MethodInfo method)
        {
            Guard.ArgumentNotNull(nameof(method), nameof(method));
            return CustomAttributeAccessor.GetCustomAttributes<IInterceptorProvider>(method)
                .Reverse()
                .ToArray();
        }

        public IInterceptorProvider[] GetInterceptorProvidersForProperty(Type targetType, PropertyInfo property, PropertyMethod propertyMethod)
        {
            Guard.ArgumentNotNull(nameof(property), nameof(property));
            var method = propertyMethod == PropertyMethod.Get ? property.GetMethod : property.SetMethod;
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