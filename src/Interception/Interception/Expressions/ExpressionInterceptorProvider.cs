using Microsoft.Extensions.Options;
using System.Linq.Expressions;
using System.Reflection;

namespace Dora.Interception.Expressions
{
    internal class ExpressionInterceptorProvider : IInterceptorProvider, IInterceptorRegistry
    {
        #region Fields
        private readonly IConventionalInterceptorFactory _conventionalInterceptorFactory;
        private readonly Dictionary<MethodInfo, List<Sortable<InvokeDelegate>>> _interceptors = new();
        private readonly HashSet<Type>_suppressedTypes = new();
        private readonly HashSet<MethodInfo> _suppressedMethods = new();
        #endregion

        #region Constructors
        public ExpressionInterceptorProvider(IConventionalInterceptorFactory conventionalInterceptorFactory, IOptions<InterceptionOptions> optionsAccessor)
        {
            _conventionalInterceptorFactory = conventionalInterceptorFactory ?? throw new ArgumentNullException(nameof(conventionalInterceptorFactory));
            var registrations = (optionsAccessor ?? throw new ArgumentNullException(nameof(optionsAccessor))).Value.InterceptorRegistrations;
            registrations?.Invoke(this);
        }
        #endregion

        #region Public methods
        public IInterceptorRegistry<TInterceptor> For<TInterceptor>(params object[] arguments)
            => new InterceptorRegistry<TInterceptor>(_interceptors, _conventionalInterceptorFactory.CreateInterceptor(typeof(TInterceptor), arguments));
        public IEnumerable<Sortable<InvokeDelegate>> GetInterceptors(MethodInfo method)
        {
            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }
            return _interceptors.TryGetValue(method, out var value)
                ? value
                : Enumerable.Empty<Sortable<InvokeDelegate>>();
        }
        public bool IsInterceptionSuppressed(MethodInfo method)
        {
            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }
            return _suppressedTypes.Contains(method.DeclaringType!) || _suppressedMethods.Contains(method);
        }
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
            private readonly Dictionary<MethodInfo, List<Sortable<InvokeDelegate>>> _interceptors;
            private readonly InvokeDelegate _interceptor;
            public InterceptorRegistry(Dictionary<MethodInfo, List<Sortable<InvokeDelegate>>> interceptors, InvokeDelegate interceptor)
            {
                _interceptors = interceptors;
                _interceptor = interceptor;
            }

            public IInterceptorRegistry<TInterceptor> ToMethod<TTarget>(int order, Expression<Action<TTarget>> methodCall)
            {
                if (methodCall == null)
                {
                    throw new ArgumentNullException(nameof(methodCall));
                }

                var method = MemberUtilities.GetMethod(methodCall);
                return ToMethod(order, method);
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
                return ToMethod(order, getMethod);
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
                    ToMethod(order, method);
                    valid = true;
                }

                method = property.SetMethod;
                if (method is not null && MemberUtilities.IsInterfaceOrVirtualMethod(method))
                {
                    ToMethod(order, method);
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
                return ToMethod(order, setMethod);
            }

            public IInterceptorRegistry<TInterceptor> ToMethod(int order, MethodInfo method)
            {
                if (method == null)
                {
                    throw new ArgumentNullException(nameof(method));
                }

                if (!MemberUtilities.IsInterfaceOrVirtualMethod(method))
                {
                    throw new InterceptionException($"Interceptor is applied to the method '{method.Name}' of type '{method.DeclaringType}', which is neither virtual method nor interface implementation method.");
                }

                var list = _interceptors.TryGetValue(method, out var value)
                    ? value
                    : _interceptors[method] = new List<Sortable<InvokeDelegate>>();
                list.Add(new Sortable<InvokeDelegate>(order, _interceptor));
                return this;
            }
        }
        #endregion
    }
}
