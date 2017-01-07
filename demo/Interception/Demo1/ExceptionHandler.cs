using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dora.Interception;
using Microsoft.Extensions.Logging;

namespace Demo1
{
    public class ExceptionHandler
    {
        private InterceptDelegate _next;
        private ILogger _logger;

        public ExceptionHandler(InterceptDelegate next, ILoggerFactory loggerFactory, string logCategory)
        {
            _next = next;
            _logger = loggerFactory.CreateLogger(logCategory);
        }

        public async Task InvokeAsync(InvocationContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }
    }
}