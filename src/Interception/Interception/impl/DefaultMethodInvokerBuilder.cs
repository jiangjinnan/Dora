using System.Reflection;

namespace Dora.Interception
{
    [NonInterceptable]

    internal class DefaultMethodInvokerBuilder : IMethodInvokerBuilder
    {
        private readonly IEnumerable<IInterceptorProvider> _interceptorProviders;
        private readonly Dictionary<Tuple<Type, MethodInfo>, Sortable<InvokeDelegate>[]> _cache = new();

        public DefaultMethodInvokerBuilder(IEnumerable<IInterceptorProvider> interceptorProviders)
        {
            _interceptorProviders = interceptorProviders ?? throw new ArgumentNullException(nameof(interceptorProviders));
        }

        public InvokeDelegate Build(Type targetType, MethodInfo method, InvokeDelegate targetMethodInvoker)
        {
            Guard.ArgumentNotNull(targetType);
            Guard.ArgumentNotNull(method);
            Guard.ArgumentNotNull(targetMethodInvoker);

            if (!CanIntercept(targetType, method))
            {
                throw new InterceptionException($"The method '{method.Name}' of '{targetType}' cannot be interceptable.");
            }

            var key = new Tuple<Type, MethodInfo>(targetType, method);

            var interceptors = _cache.TryGetValue(key, out var value)
                ? value!
                : _cache[key] = _interceptorProviders.SelectMany(it => it.GetInterceptors(targetType, method)).OrderBy(it => it.Order).ToArray();

            var length = interceptors.Length;
            interceptors = Enumerable.Range(0, length).Select(it => new Sortable<InvokeDelegate>(it, interceptors[it].Value)).ToArray();
            Array.ForEach(interceptors, Wrap);
            return interceptors[0].Value;

            void Wrap(Sortable<InvokeDelegate> sortable)
            {
                var index = sortable.Order;
                var interceptor = sortable.Value;
                sortable.Value = context =>
                {
                    context.Next = index < length - 1 ? interceptors![index + 1].Value : targetMethodInvoker;
                    return interceptor(context);
                };
            }
        }

        public bool CanIntercept(Type targetType, MethodInfo method)
        {
            Guard.ArgumentNotNull(targetType);
            Guard.ArgumentNotNull(method);
            bool interceptable = false; 
            foreach (var provider in _interceptorProviders)
            {
                if (provider.CanIntercept(targetType, method, out var suppressed))
                {
                    interceptable = true;
                }
                if (suppressed)
                {
                    return false;
                }
            }
            return interceptable;
        }
    }
}
