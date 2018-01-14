using Dora.DynamicProxy;
using Dora.Interception;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace App
{
    public class TraceInterceptor
    {
        private readonly InterceptDelegate _next;
        private readonly ILogger _logger;   
        public TraceInterceptor(InterceptDelegate next, ILoggerFactory loggerFactory)
        {
            _next = next;
            _logger = loggerFactory.CreateLogger("ddd");  
        }

        public Task InvokeAsync(InvocationContext context)
        {
            _logger.LogInformation("{0}.{1}() is invoked", context.Target.GetType().FullName, context.Method.Name);
            return _next(context);
        }
    }


    public class TraceInterceptorAttribute : InterceptorAttribute
    {
        private string _category;
        public TraceInterceptorAttribute(string category)
        {
            _category = category; 
        }
        public override void Use(IInterceptorChainBuilder builder)
        {
            builder.Use<TraceInterceptor>(this.Order);
        }
    }
}
