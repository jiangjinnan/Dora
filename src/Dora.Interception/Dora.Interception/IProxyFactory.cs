using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dora.Interception
{
    public interface IProxyFactory
    {
        IServiceProvider ServiceProvider { get; }
        object CreateProxy(Type typeToProxy, object target);
    }
}
