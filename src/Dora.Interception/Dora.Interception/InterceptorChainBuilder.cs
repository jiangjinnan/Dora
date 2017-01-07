using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dora.Interception
{
    public class InterceptorChainBuilder : IInterceptorChainBuilder
    {
        private List<Tuple<int, InterceptorDelegate>> _interceptors;
        public IServiceProvider ServiceProvider { get; }

        public InterceptorChainBuilder(IServiceProvider serviceProvider)
        {
            this.ServiceProvider = serviceProvider;
            _interceptors = new List<Tuple<int, InterceptorDelegate>>();
        }

        public IInterceptorChainBuilder Use(InterceptorDelegate interceptor, int order)
        {
            _interceptors.Add(new Tuple<int, InterceptorDelegate>(order, interceptor));
            return this;
        }

        public InterceptorDelegate Build()
        {
            var result = from it in _interceptors
                         orderby it.Item1
                         select it.Item2;
            return next => {
                var current = next;
                foreach (var it in result.Reverse())
                {
                    current = it(current);
                }
                return current;
            };
        }

        public IInterceptorChainBuilder New()
        {
            return new InterceptorChainBuilder(this.ServiceProvider);
        }
    }
}
