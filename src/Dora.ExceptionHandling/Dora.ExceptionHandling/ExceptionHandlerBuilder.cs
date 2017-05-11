using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Dora.ExceptionHandling
{
    public class ExceptionHandlerBuilder : IExceptionHandlerBuilder
    {
        private List<Func<ExceptionContext, Task>> _handlers = new List<Func<ExceptionContext, Task>>();
        public IServiceProvider ServiceProvider { get; }

        public ExceptionHandlerBuilder(IServiceProvider serviceProvider)
        {
            this.ServiceProvider = Guard.ArgumentNotNull(serviceProvider, nameof(serviceProvider));
            _handlers = new List<Func<ExceptionContext, Task>>();
        }

        public void AddHandler(Func<ExceptionContext, Task> handler)
        {
            _handlers.Add(Guard.ArgumentNotNull(handler, nameof(handler)));
        }

        public Func<ExceptionContext, Task> Build()
        {
            return async context =>
            {
                foreach (var it in _handlers)
                {
                    await it(context);
                }
            };
        }
    }
}
