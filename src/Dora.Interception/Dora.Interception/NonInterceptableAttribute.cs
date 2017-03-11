using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dora.Interception
{
    /// <summary>
    /// An attribute indicating the target method is not allowed to be intercepted.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class| AttributeTargets.Method)]
    public class NonInterceptableAttribute:Attribute
    {
    }
}
