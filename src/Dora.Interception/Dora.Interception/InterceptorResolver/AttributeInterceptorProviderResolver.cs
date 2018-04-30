using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Dora.Interception
{
    internal class AttributeInterceptorProviderResolver : IInterceptorProviderResolver
    {  
        public IInterceptorProvider[] GetInterceptorProvidersForType(Type type)
        {
            Guard.ArgumentNotNull(nameof(type), nameof(type));
            return CustomAttributeAccessor.GetCustomAttributes<IInterceptorProvider>(type)
                .Reverse()
                .ToArray();
        }

        public IInterceptorProvider[] GetInterceptorProvidersForMethod(MethodInfo method)
        {
            Guard.ArgumentNotNull(nameof(method), nameof(method));
            return CustomAttributeAccessor.GetCustomAttributes<IInterceptorProvider>(method)
                .Reverse()
                .ToArray();
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

        public bool? WillIntercept(MethodInfo method)
        {
            Guard.ArgumentNotNull(nameof(method), nameof(method));
            var attributes = CustomAttributeAccessor.GetCustomAttributes<NonInterceptableAttribute>(method, true);
            if (attributes.Any(it => it.InterceptorProviderTypes.Length == 0))
            {
                return false;
            }      
            return null;
        }

        public bool? WillIntercept(PropertyInfo property)
        {
            Guard.ArgumentNotNull(nameof(property), nameof(property));
            var attributes = CustomAttributeAccessor.GetCustomAttributes<NonInterceptableAttribute>(property, true);
            if (attributes.Any(it => it.InterceptorProviderTypes.Length == 0))
            {
                return true;
            }
            return null;
        }

        public IInterceptorProvider[] GetInterceptorProvidersForProperty(PropertyInfo property, PropertyMethod propertyMethod)
        {
            Guard.ArgumentNotNull(nameof(property), nameof(property));
            var method = propertyMethod == PropertyMethod.Get ? property.GetMethod : property.SetMethod;
            return CustomAttributeAccessor.GetCustomAttributes<IInterceptorProvider>(property).ToArray()
                .Concat(CustomAttributeAccessor.GetCustomAttributes<IInterceptorProvider>(method).ToArray());
        }

       
    }
}
