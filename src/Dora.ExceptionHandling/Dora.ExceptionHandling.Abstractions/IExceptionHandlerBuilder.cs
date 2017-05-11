using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Dora.ExceptionHandling
{
    public interface IExceptionHandlerBuilder
    {
        IServiceProvider ServiceProvider { get; }
        void AddHandler(Func<ExceptionContext, Task> handler);
        Func<ExceptionContext, Task> Build();
    }
}
