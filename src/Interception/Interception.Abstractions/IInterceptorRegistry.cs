using System;

namespace Dora.Interception
{
    public interface IInterceptorRegistry: IInterceptorRegistrationProvider
    {
        IInterceptorRegistry Register<TInterceptor>(Action<IInterceptorAssigner> assignment, params object[] arguments);
    }
}
