using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.Interception
{
    public interface IInterceptorBuilder
    {
        IInterceptor Build(IEnumerable<InterceptorRegistration> registrations);
    }
}
