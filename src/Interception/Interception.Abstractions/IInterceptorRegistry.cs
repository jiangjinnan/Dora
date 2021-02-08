using System;

namespace Dora.Interception
{
    public interface IInterceptorRegistry: IInterceptorRegistrationProvider
    {
        IInterceptorRegistry For<TInterceptor>(Action<IInterceptorAssigner> assignment, params object[] arguments);
    }
}
