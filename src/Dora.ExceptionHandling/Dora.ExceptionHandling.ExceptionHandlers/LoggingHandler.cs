using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Dora.ExceptionHandling
{
    public class LoggingHandler
    {
        public ILogger Logger { get; }
        public Func<ExceptionContext, string> Formatter { get; }
        public LoggingHandler(ILoggerFactory loggerFactory, string logCategory, Func<ExceptionContext, string> formatter)
        {
            this.Logger = Guard.ArgumentNotNull(loggerFactory, nameof(loggerFactory)).CreateLogger(Guard.ArgumentNotNullOrWhiteSpace(logCategory, nameof(logCategory)));
            this.Formatter = Guard.ArgumentNotNull(formatter, nameof(formatter));
        }

        public Task HandleExceptionAsync(ExceptionContext context)
        {
            Guard.ArgumentNotNull(context, nameof(context));
            this.Logger.LogError(this.Formatter(context));
            return Task.CompletedTask;
        }
    }
}