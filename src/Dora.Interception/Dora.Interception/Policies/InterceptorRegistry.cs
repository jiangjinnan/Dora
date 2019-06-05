//using Dora.Interception.Properties;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Reflection;
//using System.Text;

//namespace Dora.Interception
//{
//    public class InterceptorRegistry
//    {
//        public IDictionary<Type, InterceptorProviderRegistry> Registries { get; }
//        public IServiceProvider ServiceProvider { get; }

//        public InterceptorRegistry(IServiceProvider serviceProvider)
//        {
//            ServiceProvider = Guard.ArgumentNotNull(serviceProvider, nameof(serviceProvider));
//            Registries = new Dictionary<Type, InterceptorProviderRegistry>();
//        }
//        public InterceptorProviderRegistry<TInterceptorProvider> For<TInterceptorProvider>(int order, params object[] arguments)
//            where TInterceptorProvider : IInterceptorProvider
//        {

//            Func<IInterceptorProvider> factory = () =>
//            {
//                var provider = Microsoft.Extensions.DependencyInjection.ActivatorUtilities.CreateInstance<TInterceptorProvider>(ServiceProvider, arguments);
//                var orderedElement = provider as IOrderedSequenceItem;
//                if (null != orderedElement)
//                {
//                    orderedElement.Order = order;
//                }
//                return provider;
//            };
//            var registry = new InterceptorProviderRegistry<TInterceptorProvider>(factory);
//            Registries[typeof(TInterceptorProvider)] = registry;
//            return registry;
//        }
//    }

//    public abstract class InterceptorProviderRegistry
//    {
//        public abstract Func<IInterceptorProvider> InterceptorProviderFactory { get; }
//        public abstract InterceptorProviderRegistration GetRegistrations();
//    }
//    public class InterceptorProviderRegistry<TInterceptorProvider> : InterceptorProviderRegistry
//        where TInterceptorProvider : IInterceptorProvider
//    {
//        private Dictionary<Type, TargetTypeInterceptorRegistry> _targetTypeInterceptorRegistries;

//        public InterceptorProviderRegistry(Func<IInterceptorProvider> interceptorProviderFactory)
//        {
//            InterceptorProviderFactory = Guard.ArgumentNotNull(interceptorProviderFactory, nameof(interceptorProviderFactory));
//            _targetTypeInterceptorRegistries = new Dictionary<Type, TargetTypeInterceptorRegistry>();
//        }

//        public override Func<IInterceptorProvider> InterceptorProviderFactory { get; }

//        public override InterceptorProviderRegistration GetRegistrations()
//        {
//            var targetRegistrations = _targetTypeInterceptorRegistries.Values.Select(it => it.ToRegistration());
//            return new InterceptorProviderRegistration(typeof(TInterceptorProvider), InterceptorProviderFactory, targetRegistrations);
//        }

//        public TargetTypeInterceptorRegistry<TTarget> Target<TTarget>()
//            where TTarget : class
//        {
//            var returnValue = _targetTypeInterceptorRegistries.TryGetValue(typeof(TTarget), out var registry)
//            ? registry
//            : _targetTypeInterceptorRegistries[typeof(TTarget)] = new TargetTypeInterceptorRegistry<TTarget>();
//            return (TargetTypeInterceptorRegistry<TTarget>)returnValue;
//        }
//    }

//    public abstract class TargetTypeInterceptorRegistry
//    {
//        public Type TargetType { get; }
//        public abstract TargetRegistration ToRegistration();
//        public TargetTypeInterceptorRegistry(Type targetType)
//        {
//            TargetType = Guard.ArgumentNotNull(targetType, nameof(targetType));
//        }
//    }

//    public class TargetTypeInterceptorRegistry<TTarget> : TargetTypeInterceptorRegistry
//        where TTarget : class
//    {
//        private TargetRegistration _registration;
//        public TargetTypeInterceptorRegistry() : base(typeof(TTarget))
//        {
//            _registration = new TargetRegistration(typeof(TTarget));
//        }

//        public TargetTypeInterceptorRegistry<TTarget> IncludeAllMembers()
//        {
//            _registration.IncludedAllMembers = true;
//            return this;
//        }
//        public TargetTypeInterceptorRegistry<TTarget> ExcludeAllMembers()
//        {
//            _registration.ExludedAllMembers = true;
//            return this;
//        }
//        public TargetTypeInterceptorRegistry<TTarget> IncludeMethod(Expression<Action<TTarget>> expression)
//            => RegisterMethod(expression, true);
//        public TargetTypeInterceptorRegistry<TTarget> ExcludeMethod(Expression<Action<TTarget>> expression)
//            => RegisterMethod(expression, false);
//        public TargetTypeInterceptorRegistry<TTarget> IncludeProperty<TProperty>(Expression<Func<TTarget, TProperty>> expression, PropertyMethod propertyMethod)
//            => RegisterProperty(expression, propertyMethod, true);
//        public TargetTypeInterceptorRegistry<TTarget> ExcludeProperty<TProperty>(Expression<Func<TTarget, TProperty>> expression, PropertyMethod propertyMethod)
//            => RegisterProperty(expression, propertyMethod, false);

//        public override TargetRegistration ToRegistration() => _registration;

//        private TargetTypeInterceptorRegistry<TTarget> RegisterMethod(Expression<Action<TTarget>> expression, bool include)
//        {
//            Guard.ArgumentNotNull(expression, nameof(expression));
//            var methodCall = expression.Body as MethodCallExpression;
//            if (methodCall == null)
//            {
//                throw new ArgumentException(Resources.NotMethodCallExpression, nameof(expression));
//            }
//            var method = methodCall.Method;
//            if (method.IsGenericMethod)
//            {
//                method = method.GetGenericMethodDefinition();
//            }
//            if (include)
//            {
//                _registration.IncludedMethods.Add(method.MetadataToken);
//            }
//            else
//            {
//                _registration.ExludedMethods.Add(method.MetadataToken);
//            }
//            return this;
//        }

//        private TargetTypeInterceptorRegistry<TTarget> RegisterProperty<TProperty>(Expression<Func<TTarget, TProperty>> expression, PropertyMethod propertyMethod, bool include)
//        {
//            Guard.ArgumentNotNull(expression, nameof(expression));
//            var memberExpression = expression.Body as MemberExpression;
//            var property = memberExpression?.Member as PropertyInfo;
//            if (property == null)
//            {
//                throw new ArgumentException(Resources.NotPropertyAccessExpression, nameof(expression));
//            }
//            if (include)
//            {
//                _registration.IncludedProperties.Add(property.MetadataToken, propertyMethod);
//            }
//            else
//            {
//                _registration.ExludedProperties.Add(property.MetadataToken, propertyMethod);
//            }
//            return this;
//        }
//    }
//}
