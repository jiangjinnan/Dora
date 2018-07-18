using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.Interception
{
    internal class InterceptorProviderRegistrationBuilder : IInterceptorProviderRegistrationBuilder
    {
        private readonly List<TargetRegistration> _registrations;     
        public InterceptorProviderRegistrationBuilder()
        {
            _registrations = new List<TargetRegistration>();
        }

        public IEnumerable<TargetRegistration> Build() => _registrations;

        public IInterceptorProviderRegistrationBuilder Target<T>(Action<ITargetRegistrationBuilder<T>> configure)
        {
            var builder = new TargetRegistrationBuilder<T>();
            configure.Invoke(builder);
            _registrations.Add(builder.Build());
            return this;
        }
    }
}
