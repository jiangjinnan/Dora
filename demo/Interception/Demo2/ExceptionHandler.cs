using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dora.Interception;
using Microsoft.Extensions.Logging;

namespace Demo2
{
    public class ExceptionHandler
    {
        private InterceptDelegate _next;
        private string _logCategory;

        public ExceptionHandler(InterceptDelegate next,  string logCategory)
        {
            _next = next;
            _logCategory = logCategory;
        }

        public async Task InvokeAsync(InvocationContext context, ILoggerFactory loggerFactory)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                loggerFactory.CreateLogger(_logCategory).LogError(ex.Message);
            }
        }
    }
}