using Dora.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dora.Interception
{
    /// <summary>
    /// The default implementation of interceptor chain builder.
    /// </summary>
    public class InterceptorChainBuilder : IInterceptorChainBuilder
    {
        private List<Tuple<int, InterceptorDelegate>> _interceptors;

        /// <summary>
        /// Gets the service provider to get dependency services.
        /// </summary>
        public IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Create a new <see cref="InterceptorChainBuilder"/>.
        /// </summary>
        /// <param name="serviceProvider">The service provider to get dependency services.</param>
        /// <exception cref="ArgumentNullException">The argument <paramref name="serviceProvider"/> is null.</exception>
        public InterceptorChainBuilder(IServiceProvider serviceProvider)
        {
            Guard.ArgumentNotNull(serviceProvider, nameof(serviceProvider));

            this.ServiceProvider = serviceProvider;
            _interceptors = new List<Tuple<int, InterceptorDelegate>>();
        }


        /// <summary>
        /// Register specified interceptor.
        /// </summary>
        /// <param name="interceptor">The interceptor to register.</param>
        /// <param name="order">The order for the registered interceptor in the interceptor chain.</param>
        /// <returns>The interceptor chain builder with registered intercetor.</returns>
        /// <exception cref="ArgumentNullException">The argument <paramref name="interceptor"/> is null.</exception>
        public IInterceptorChainBuilder Use(InterceptorDelegate interceptor, int order)
        {
            Guard.ArgumentNotNull(interceptor, nameof(interceptor));
            _interceptors.Add(new Tuple<int, InterceptorDelegate>(order, interceptor));
            return this;
        }

        /// <summary>
        /// Build an interceptor chain using the registerd interceptors.
        /// </summary>
        /// <returns>A composite interceptor representing the interceptor chain.</returns>
        public InterceptorDelegate Build()
        {
            if (_interceptors.Count == 0)
            {
                return next => (_ => Task.CompletedTask);
            }

            if (_interceptors.Count == 1)
            {
                return _interceptors.Single().Item2;
            }

            var interceptors = _interceptors
                 .OrderBy(it => it.Item1)
                 .Select(it=>it.Item2)
                 .Reverse()
                 .ToArray(); 

            return next => {
                var current = next;
                for (int index = 0; index < interceptors.Length; index++)
                {
                    current = interceptors[index](current);
                }  
                return current;
            };
        }

        /// <summary>
        /// Create a new interceptor chain builder which owns the same service provider as the current one.
        /// </summary>
        /// <returns>The new interceptor to create.</returns>
        public IInterceptorChainBuilder New()
        {
            return new InterceptorChainBuilder(this.ServiceProvider);
        }  
    }
}
