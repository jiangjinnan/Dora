using System.Reflection;

namespace Dora.Interception
{
    [NonInterceptable]
    internal class DataAnnotationInterceptorProvider : InterceptorProviderBase
    {
        private readonly BindingFlags _bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        public DataAnnotationInterceptorProvider(IConventionalInterceptorFactory interceptorFactory) : base(interceptorFactory)
        {
        }

        public override IEnumerable<Sortable<InvokeDelegate>> GetInterceptors(Type targetType, MethodInfo method)
        {
            if (targetType == null) throw new ArgumentNullException(nameof(targetType));
            if (method == null) throw new ArgumentNullException(nameof(method));

            var list = new List<Sortable<InvokeDelegate>>();
            foreach (var attribute in targetType.GetCustomAttributes<InterceptorAttribute>())
            {
                list.Add(new Sortable<InvokeDelegate>(attribute.Order, CreateInterceptor(attribute)));
            }

            foreach (var attribute in method.GetCustomAttributes<InterceptorAttribute>())
            {
                list.Add(new Sortable<InvokeDelegate>(attribute.Order, CreateInterceptor(attribute)));
            }

            if (method.IsSpecialName && (method.Name.StartsWith("get_") || method.Name.StartsWith("set_")))
            {
                if (MemberUtilities.TryGetProperty(method, out var property))
                {
                    foreach (var attribute in property!.GetCustomAttributes<InterceptorAttribute>())
                    {
                        list.Add(new Sortable<InvokeDelegate>(attribute.Order, CreateInterceptor(attribute)));
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

            var isPublic = type.IsPublic;
            var attribute = type.GetCustomAttributes<InterceptorAttribute>().FirstOrDefault();
            if (!isPublic && attribute is not null)
            {
                throw new InterceptionException($"The interceptor '{attribute.Interceptor}' must not be applied to non-public type '{type}'.");
            }           

            foreach (var method in type.GetMethods(_bindingFlags))
            {
                attribute = method.GetCustomAttributes<InterceptorAttribute>().FirstOrDefault();
                if (attribute is not null)
                {
                    if (!isPublic)
                    {
                        throw new InterceptionException($"The interceptor '{attribute.Interceptor}' must not be applied to non-public type '{type}'.");
                    }
                    methodValidator(method);
                }                   
            }

            foreach (var property in type.GetProperties(_bindingFlags))
            {
                attribute = property.GetCustomAttributes<InterceptorAttribute>().FirstOrDefault();
                if (attribute is not null)
                {
                    if (!isPublic)
                    {
                        throw new InterceptionException($"The interceptor '{attribute.Interceptor}' must not be applied to non-public type '{type}'.");
                    }
                    propertyValidator(property);
                }
            }
        }

        private readonly Dictionary<Assembly, bool> _suppressedAssemblies = new();
        public override bool CanIntercept(Type targetType, MethodInfo method, out bool suppressed)
        {
            if (targetType == null) throw new ArgumentNullException(nameof(targetType));
            if (method == null) throw new ArgumentNullException(nameof(method));
            PropertyInfo? property = null;

            var assembly = targetType.Assembly;
            if (_suppressedAssemblies.TryGetValue(assembly, out var sup) && sup)
            {
                suppressed = true;
                return false;
            }
            else
            {
                sup = assembly.GetCustomAttributes<NonInterceptableAttribute>().Any();
                _suppressedAssemblies[assembly] = sup;
                if (sup)
                {
                    suppressed = true;
                    return false;
                }
            }

            if (method.IsSpecialName && (method.Name.StartsWith("get_") || method.Name.StartsWith("set_")))
            {
                if (MemberUtilities.TryGetProperty(method, out property) && property!.GetCustomAttributes<NonInterceptableAttribute>().Any())
                {
                    suppressed = true;
                    return false;
                }
            }

            if (targetType.GetCustomAttributes<NonInterceptableAttribute>().Any() || method.GetCustomAttributes<NonInterceptableAttribute>().Any())
            {
                suppressed = true;
                return false;
            }

            suppressed = false;
            return method.GetCustomAttributes<InterceptorAttribute>().Any()|| targetType.GetCustomAttributes<InterceptorAttribute>().Any() || (property?.GetCustomAttributes()?.Any() ?? false);
        }

        private InvokeDelegate CreateInterceptor(InterceptorAttribute interceptorAttribute)
            => interceptorAttribute.Interceptor == interceptorAttribute.GetType()
            ? InterceptorFactory.CreateInterceptor(interceptorAttribute)
            : InterceptorFactory.CreateInterceptor(interceptorAttribute.Interceptor, interceptorAttribute.Arguments);
    }
}
