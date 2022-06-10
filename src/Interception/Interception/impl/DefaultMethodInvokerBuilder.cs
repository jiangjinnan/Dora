using System.Reflection;

namespace Dora.Interception
{
    internal class DefaultMethodInvokerBuilder : IMethodInvokerBuilder
    {
        private readonly IEnumerable<IInterceptorProvider> _interceptorProviders;
        private readonly Dictionary<MethodInfo, Sortable<InvokeDelegate>[]> _cache = new();

        public DefaultMethodInvokerBuilder(IEnumerable<IInterceptorProvider> interceptorProviders)
        {
            _interceptorProviders = interceptorProviders ?? throw new ArgumentNullException(nameof(interceptorProviders));
        }

        public InvokeDelegate Build(MethodInfo method, InvokeDelegate targetMethodInvoker)
        {
            Guard.ArgumentNotNull(method);
            Guard.ArgumentNotNull(targetMethodInvoker);

            if (!CanIntercept(method))
            {
                throw new InterceptionException($"The method '{method.Name}' of '{method.DeclaringType}' cannot be interceptable.");
            }

            var interceptors = _cache.TryGetValue(method, out var value)
                ? value!
                : _cache[method] = _interceptorProviders.SelectMany(it => it.GetInterceptors(method)).OrderBy(it=>it.Order).ToArray();
        
            var length = interceptors.Length;
            interceptors = Enumerable.Range(0, length).Select(it => new Sortable<InvokeDelegate>(it, interceptors[it].Value)).ToArray();
            Array.ForEach(interceptors, Wrap);
            return interceptors[0].Value;

            void Wrap(Sortable<InvokeDelegate> sortable)
            {
                var index = sortable.Order;
                var interceptor = sortable.Value;
                sortable.Value = context => {
                    context.Next = index < length - 1 ? interceptors![index + 1].Value : targetMethodInvoker;
                    return interceptor(context);
                };
            }
        }

        public bool CanIntercept(MethodInfo method)
        {
            Guard.ArgumentNotNull(method);
            var type = method.DeclaringType!;
            foreach (var provider in _interceptorProviders)
            {
                if (provider.IsInterceptionSuppressed( method))
                {
                    return false;
                }
            }

            return  _interceptorProviders.Any(it => it.GetInterceptors(method).Any());
        }
    }
}
