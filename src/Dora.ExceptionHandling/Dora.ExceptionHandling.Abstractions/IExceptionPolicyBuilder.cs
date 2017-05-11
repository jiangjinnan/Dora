using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.ExceptionHandling
{
    public interface IExceptionPolicyBuilder
    {
        IServiceProvider ServiceProvider { get; }
        void AddHandlers(Type exceptionType, PostHandlingAction postHandlerAction, Action<IExceptionHandlerBuilder> configure);
        void AddPreHandlers(Func<Exception, bool> predicate, Action<IExceptionHandlerBuilder> configure);
        void AddPostHandlers(Func<Exception, bool> predicate, Action<IExceptionHandlerBuilder> configure);

        IExceptionPolicy Build();
    }
}
