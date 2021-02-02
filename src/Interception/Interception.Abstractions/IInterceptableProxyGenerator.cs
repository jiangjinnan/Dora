using System;

namespace Dora.Interception
{
    public interface IInterceptableProxyGenerator
    {
        Type Generate(Type serviceType, Type implementationType);
    }
}
