using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dora.Interception
{
    public interface IInterceptorProvider
    {
        void Use(IInterceptorChainBuilder builder);
        bool AllowMultiple { get; }
    }
}
