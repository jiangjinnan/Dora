using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dora.Interception
{
    public delegate Task InterceptDelegate(InvocationContext context);
}
