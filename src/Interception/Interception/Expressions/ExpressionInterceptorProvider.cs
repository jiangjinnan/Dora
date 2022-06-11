using Microsoft.Extensions.Options;
using System.Linq.Expressions;
using System.Reflection;

namespace Dora.Interception.Expressions
{
    internal class ExpressionInterceptorProvider : IInterceptorProvider, IInterceptorRegistry
    {
        #region Fields
        private readonly IConventionalInterceptorFactory _conventionalInterceptorFactory;
        private readonly Dictionary<Tuple<Type, MethodInfo>, List<Func<Sortable<InvokeDelegate>>>> _interceptorsAccessors = new();
        private readonly Dictionary<Tuple<Type,MethodInfo>, List<Sortable<InvokeDelegate>>> _interceptors = new();
        private readonly HashSet<Type>_suppressedTypes = new();
        private readonly HashSet<MethodInfo> _suppressedMethods = new();
        private readonly HashSet<Tuple<Type, MethodInfo>> _interceptedMethods = new();
        #endregion

        #region Constructors
        public ExpressionInterceptorProvider(IConventionalInterceptorFactory conventionalInterceptorFactory, IOptions<InterceptionOptions> optionsAccessor)
        {
            _conventionalInterceptorFactory = conventionalInterceptorFactory ?? throw new ArgumentNullException(nameof(conventionalInterceptorFactory));
            var registrations = (optionsAccessor ?? throw new ArgumentNullException(nameof(optionsAccessor))).Value.InterceptorRegistrations;
            registrations?.Invoke(this);
        }

        public bool CanIntercept(Type targetType, MethodInfo method)
        {
            Guard.ArgumentNotNull(targetType);
            Guard.ArgumentNotNull(method);
            if (_suppressedTypes.Contains(targetType) || _suppressedMethods.Contains(method))
            {
                return false;
            }
            return _interceptedMethods.Contains(new Tuple<Type, MethodInfo>(targetType, method));
        }
        #endregion

        #region Public methods
        public IInterceptorRegistry<TInterceptor> For<TInterceptor>(params object[] arguments)
            => new InterceptorRegistry<TInterceptor>(_interceptorsAccessors, _interceptedMethods,()=> _conventionalInterceptorFactory.CreateInterceptor(typeof(TInterceptor), arguments));
        public IEnumerable<Sortable<InvokeDelegate>> GetInterceptors(Type targetType, MethodInfo method)
        {
            Guard.ArgumentNotNull(targetType);
            Guard.ArgumentNotNull(method);
            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }

            var key = new Tuple<Type, MethodInfo>(targetType, method);
            if (_interceptors.TryGetValue(key, out var interceptors))
            { 
                return interceptors;
            }

            if (_interceptorsAccessors.TryGetValue(key, out var interceptorAccessors))
            {
                interceptors = interceptorAccessors.Select(it=>it()).ToList();
                _interceptors[key] = interceptors;
                return interceptors;

            }
            return Enumerable.Empty<Sortable<InvokeDelegate>>();
        }
        //public bool IsInterceptionSuppressed(MethodInfo method)
        //{
        //    if (method == null)
        //    {
        //        throw new ArgumentNullException(nameof(method));
        //    }
        //    return _suppressedTypes.Contains(method.DeclaringType!) || _suppressedMethods.Contains(method);
        //}
        public IInterceptorRegistry SupressGetMethod<TTarget>(Expression<Func<TTarget, object>> propertyAccessor)
        {
            var property = MemberUtilities.GetProperty(propertyAccessor);
            var method = property.GetMethod;
            if (method is not null)
            {
                _suppressedMethods.Add(method);
                return this;
            }
            throw new ArgumentException($"Specified property '{property.Name}' of '{typeof(TTarget)}' does not have Get method.", nameof(propertyAccessor));
        }
        public IInterceptorRegistry SupressMethod<TTarget>(Expression<Action<TTarget>> methodCall)
        {
            if(methodCall == null) throw new ArgumentNullException(nameof(methodCall));
            var method = MemberUtilities.GetMethod(methodCall);
            _suppressedMethods.Add(method);
            return this;
        }
        public IInterceptorRegistry SupressMethods(params MethodInfo[] methods)
        {
            Array.ForEach(methods, it => _suppressedMethods.Add(it));
            return this;
        }
        public IInterceptorRegistry SupressProperty<TTarget>(Expression<Func<TTarget, object>> propertyAccessor)
        {
            var property = MemberUtilities.GetProperty(propertyAccessor);
            var method = property.GetMethod;
            if (method is not null)
            {
                _suppressedMethods.Add(method);
            }
            method = property.SetMethod;
            if (method is not null)
            {
                _suppressedMethods.Add(method);
            }
            return this;
        }
        public IInterceptorRegistry SupressSetMethod<TTarget>(Expression<Func<TTarget, object>> propertyAccessor)
        {
            var property = MemberUtilities.GetProperty(propertyAccessor);
            var method = property.SetMethod;
            if (method is not null)
            {
                _suppressedMethods.Add(method);
                return this;
            }
            throw new ArgumentException($"Specified property '{property.Name}' of '{typeof(TTarget)}' does not have Set method.", nameof(propertyAccessor));
        }
        public IInterceptorRegistry SupressType<TTarget>()
        {
            _suppressedTypes.Add(typeof(TTarget));
            return this;
        }
        public IInterceptorRegistry SupressTypes(params Type[] types)
        {
            Array.ForEach(types, it => _suppressedTypes.Add(it));
            return this;
        }
        #endregion

        #region Private methods
        private sealed class InterceptorRegistry<TInterceptor> : IInterceptorRegistry<TInterceptor>
        {
            private readonly Dictionary<Tuple<Type, MethodInfo>, List<Func<Sortable<InvokeDelegate>>>> _interceptorAccessors;
            private readonly Func<InvokeDelegate> _interceptorFatory;
            private readonly HashSet<Tuple<Type, MethodInfo>> _interceptedMethods;
            public InterceptorRegistry(Dictionary<Tuple<Type, MethodInfo>, List<Func<Sortable<InvokeDelegate>>>> interceptorAccessors, HashSet<Tuple<Type, MethodInfo>> interceptedMethods, Func<InvokeDelegate> interceptorFatory)
            {
                _interceptorAccessors = interceptorAccessors;
                _interceptedMethods = interceptedMethods;
                _interceptorFatory = interceptorFatory;
            }

            public IInterceptorRegistry<TInterceptor> ToMethod<TTarget>(int order, Expression<Action<TTarget>> methodCall)
            {
                if (methodCall == null)
                {
                    throw new ArgumentNullException(nameof(methodCall));
                }

                var method = MemberUtilities.GetMethod(methodCall);
                return ToMethod(order, typeof(TTarget), method);
            }

            public IInterceptorRegistry<TInterceptor> ToGetMethod<TTarget>(int order, Expression<Func<TTarget, object>> propertyAccessor)
            {
                if (propertyAccessor == null)
                {
                    throw new ArgumentNullException(nameof(propertyAccessor));
                }
                var property = MemberUtilities.GetProperty(propertyAccessor);
                var getMethod = property.GetMethod;
                if (getMethod is null)
                {
                    throw new InterceptionException($"Specified property '{property.Name}' of '{property.DeclaringType}' does not have Get method.");
                }
                return ToMethod(order, typeof(TTarget), getMethod);
            }

            public IInterceptorRegistry<TInterceptor> ToProperty<TTarget>(int order, Expression<Func<TTarget, object>> propertyAccessor)
            {
                if (propertyAccessor == null)
                {
                    throw new ArgumentNullException(nameof(propertyAccessor));
                }
                var property = MemberUtilities.GetProperty(propertyAccessor);
                var method = property.GetMethod;
                var valid = false;
                if (method is not null && MemberUtilities.IsInterfaceOrVirtualMethod(method))
                {
                    ToMethod(order, typeof(TTarget), method);
                    valid = true;
                }

                method = property.SetMethod;
                if (method is not null && MemberUtilities.IsInterfaceOrVirtualMethod(method))
                {
                    ToMethod(order, typeof(TTarget), method);
                    valid = true;
                }

                if (!valid)
                {
                    throw new InterceptionException($"Interceptor is applied to the property '{property.Name}' of type '{typeof(TTarget)}', whose Get/Set methods are not virtual method or interface implementation method.");
                }

                return this;
            }

            public IInterceptorRegistry<TInterceptor> ToSetMethod<TTarget>(int order, Expression<Func<TTarget, object>> propertyAccessor)
            {
                if (propertyAccessor == null)
                {
                    throw new ArgumentNullException(nameof(propertyAccessor));
                }
                var property = MemberUtilities.GetProperty(propertyAccessor);
                var setMethod = property.SetMethod;
                if (setMethod is null)
                {
                    throw new InterceptionException($"Specified property '{property.Name}' of '{property.DeclaringType}' does not have Get method.");
                }
                return ToMethod(order,typeof(TTarget), setMethod);
            }

            public IInterceptorRegistry<TInterceptor> ToMethod(int order, Type targetType, MethodInfo method)
            {
                if (method == null)
                {
                    throw new ArgumentNullException(nameof(method));
                }

                if (!MemberUtilities.IsInterfaceOrVirtualMethod(method))
                {
                    throw new InterceptionException($"Interceptor is applied to the method '{method.Name}' of type '{targetType}', which is neither virtual method nor interface implementation method.");
                }
                var key = new Tuple<Type, MethodInfo>(targetType, method);
                _interceptedMethods.Add(key);
                var list = _interceptorAccessors.TryGetValue(key, out var value)
                    ? value
                    : _interceptorAccessors[key] = new List<Func<Sortable<InvokeDelegate>>>();
                list.Add(()=>new Sortable<InvokeDelegate>(order, _interceptorFatory()));
                return this;
            }
        }
        #endregion
    }
}
