using System.Collections.Generic;

namespace Dora.Interception
{
    public interface IInterceptorRegistrationProvider
    {
        IEnumerable<InterceptorRegistration> Registrations { get; }
    }
}
