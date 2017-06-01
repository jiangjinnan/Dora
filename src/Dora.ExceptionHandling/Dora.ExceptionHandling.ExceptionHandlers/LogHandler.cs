using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Dora.ExceptionHandling
{
    /// <summary>
    /// An exception handler to log exception information.
    /// </summary>
    public class LogHandler
    {
        /// <summary>
        /// The <see cref="ILogger"/> used to log the formatted exception information.
        /// </summary>
        public ILogger Logger { get; }

        /// <summary>
        /// A <see cref="Func{ExceptionContext, String} "/> to generate a formatted log message.
        /// </summary>
        public Func<ExceptionContext, string> Formatter { get; }

        /// <summary>
        /// Creates a new <see cref="LogHandler"/>.
        /// </summary>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> to create a the <see cref="ILogger"/>.</param>
        /// <param name="logCategory">The category of log entry to write.</param>
        /// <param name="formatter">A <see cref="Func{ExceptionContext, String} "/> to generate a formatted log message.</param>
        /// <exception cref="ArgumentNullException">The specified <paramref name="loggerFactory"/> is null.</exception>
        /// <exception cref="ArgumentNullException">The specified <paramref name="logCategory"/> is null.</exception>
        /// <exception cref="ArgumentNullException">The specified <paramref name="formatter"/> is null.</exception>
        /// <exception cref="ArgumentException">The specified <paramref name="logCategory"/> is a white space string.</exception>
        public LogHandler(ILoggerFactory loggerFactory, string logCategory, Func<ExceptionContext, string> formatter)
        {
            Guard.ArgumentNotNull(loggerFactory, nameof(loggerFactory));
            Guard.ArgumentNotNullOrWhiteSpace(logCategory, nameof(logCategory));
            this.Logger = loggerFactory.CreateLogger(logCategory);
            this.Formatter = Guard.ArgumentNotNull(formatter, nameof(formatter));
        }

        /// <summary>
        /// Handle the exception.
        /// </summary>
        /// <param name="context">The <see cref="ExceptionContext"/> representing the execution context for exception handling.</param>
        /// <returns>The task to handle the exception.</returns>
        /// <exception cref="ArgumentNullException">The specified <paramref name="context"/> is null.</exception>
        public Task HandleExceptionAsync(ExceptionContext context)
        {
            Guard.ArgumentNotNull(context, nameof(context));
            this.Logger.LogError(this.Formatter(context));
            return Task.CompletedTask;
        }
    }
}