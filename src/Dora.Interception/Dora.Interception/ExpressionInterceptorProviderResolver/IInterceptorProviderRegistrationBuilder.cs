using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.Interception
{
    public interface IInterceptorProviderRegistrationBuilder
    {
        IEnumerable<TargetRegistration> Build();
        IInterceptorProviderRegistrationBuilder Target<T>(Action<ITargetRegistrationBuilder<T>> configure);
    }
}
