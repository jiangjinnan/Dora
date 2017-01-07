using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dora.Interception
{
    public interface IInterceptorChainBuilder
    {
        IServiceProvider ServiceProvider { get; }
        IInterceptorChainBuilder Use(InterceptorDelegate interceptor, int order);
        InterceptorDelegate Build();
        IInterceptorChainBuilder New();
    }
}
