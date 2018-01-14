using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.DynamicProxy
{
    public interface DynamicProxyBuilder
    {
        object Create(Type type);   
        object Wrap(Type type, object target);
    }
}
