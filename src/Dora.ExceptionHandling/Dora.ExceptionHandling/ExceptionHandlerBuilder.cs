using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dora.ExceptionHandling
{
    /// <summary>
    /// The default implementation of exception handler builder.
    /// </summary>
    public class ExceptionHandlerBuilder : IExceptionHandlerBuilder
    {
        private List<Func<ExceptionContext, Task>> _handlers = new List<Func<ExceptionContext, Task>>();

        /// <summary>
        /// A <see cref="IServiceProvider"/> to provide neccessary dependent services.
        /// </summary>
        public IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Create a new <see cref="ExceptionHandlerBuilder"/>.
        /// </summary>
        /// <param name="serviceProvider">A <see cref="IServiceProvider"/> to provide neccessary dependent services.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="serviceProvider"/> is null.</exception>
        public ExceptionHandlerBuilder(IServiceProvider serviceProvider)
        {
            this.ServiceProvider = Guard.ArgumentNotNull(serviceProvider, nameof(serviceProvider));
            _handlers = new List<Func<ExceptionContext, Task>>();
        }

        /// <summary>
        /// Regiser an exception handler represented by a <see cref="Func{ExceptionContext, Task}"/>.
        /// </summary>
        /// <param name="handler">A <see cref="Func{ExceptionContext, Task}"/> representing an exception handler to register.</param>
        /// <returns>The current <see cref="IExceptionHandlerBuilder"/> with registered exception handler.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="handler"/> is null.</exception>
        public IExceptionHandlerBuilder Use(Func<ExceptionContext, Task> handler)
        {
            _handlers.Add(Guard.ArgumentNotNull(handler, nameof(handler)));
            return this;
        }

        /// <summary>
        /// Build an exception handler chain.
        /// </summary>
        /// <returns>A <see cref="Func{ExceptionContext, Task}"/> representing the exception handler chain.</returns>
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