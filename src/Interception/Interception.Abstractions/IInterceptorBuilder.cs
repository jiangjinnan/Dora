using System.Collections.Generic;

namespace Dora.Interception
{
    public interface IInterceptorBuilder
    {
        IInterceptor Build(IEnumerable<InterceptorRegistration> registrations);
    }
}
