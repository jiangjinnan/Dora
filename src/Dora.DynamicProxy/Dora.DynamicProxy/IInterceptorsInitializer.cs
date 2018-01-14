using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.DynamicProxy
{
    public interface IInterceptorsInitializer
    {
        void SetInterceptors(InterceptorDecoration interceptors);
    }
}
