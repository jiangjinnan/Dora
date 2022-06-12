using System.Reflection;

namespace Dora.Interception
{
    internal class DataAnnotationInterceptorProvider : IInterceptorProvider
    {
        private readonly BindingFlags _bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        public IEnumerable<Sortable<InvokeDelegate>> GetInterceptors(Type targetType, MethodInfo method, Func<Type, object[], InvokeDelegate> interceptorFactory)
        {
            if (targetType == null) throw new ArgumentNullException(nameof(targetType));
            if (method == null) throw new ArgumentNullException(nameof(method));
            if (interceptorFactory == null) throw new ArgumentNullException(nameof(interceptorFactory));

            var list = new List<Sortable<InvokeDelegate>>();
            foreach (var attribute in targetType.GetCustomAttributes<InterceptorAttribute>())
            {
                list.Add(new Sortable<InvokeDelegate>(attribute.Order,  interceptorFactory(attribute.Interceptor, attribute.Arguments)));
            }

            foreach (var attribute in method.GetCustomAttributes<InterceptorAttribute>())
            {
                list.Add(new Sortable<InvokeDelegate>(attribute.Order, interceptorFactory(attribute.Interceptor, attribute.Arguments)));
            }

            if (method.IsSpecialName && (method.Name.StartsWith("get_") || method.Name.StartsWith("set_")))
            {
                if (MemberUtilities.TryGetProperty(method, out var property))
                {
                    foreach (var attribute in property!.GetCustomAttributes<InterceptorAttribute>())
                    {
                        list.Add(new Sortable<InvokeDelegate>(attribute.Order, interceptorFactory(attribute.Interceptor, attribute.Arguments)));
                    }
                }
            }
            return list;
        }

        public void Validate(Type type, Action<MethodInfo> methodValidator, Action<PropertyInfo> propertyValidator)
        {
            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            foreach (var method in type.GetMethods(_bindingFlags).Where(it=>it.GetCustomAttribute<InterceptorAttribute>() is not null))
            {
                methodValidator(method);
               
            }

            foreach (var property in type.GetProperties(_bindingFlags).Where(it => it.GetCustomAttribute<InterceptorAttribute>() is not null))
            {
                propertyValidator(property);

            }
        }

        public bool CanIntercept(Type targetType, MethodInfo method)
        {
            if (targetType == null) throw new ArgumentNullException(nameof(targetType));
            if (method == null) throw new ArgumentNullException(nameof(method));
            PropertyInfo? property = null;

            if (method.IsSpecialName && (method.Name.StartsWith("get_") || method.Name.StartsWith("set_")))
            {
                if (MemberUtilities.TryGetProperty(method, out property) && property!.GetCustomAttribute<NonInterceptableAttribute>() is not null)
                {
                    return false;
                }
            }

            if (targetType.GetCustomAttribute<NonInterceptableAttribute>() is not null || method.GetCustomAttribute<NonInterceptableAttribute>() is not null)
            {
                return false;
            }

            return method.GetCustomAttributes<InterceptorAttribute>().Any()|| targetType.GetCustomAttributes<InterceptorAttribute>().Any() || (property?.GetCustomAttributes()?.Any() ?? false);
        }
    }
}
