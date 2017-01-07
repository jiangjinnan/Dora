using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dora.Interception
{
    [AttributeUsage(AttributeTargets.Class| AttributeTargets.Method)]
    public class NonInterceptableAttribute:Attribute
    {
    }
}
