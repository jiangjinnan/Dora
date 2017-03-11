using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace Dora.Interception.AspNetCore
{
    public class InterceptionServiceProviderMiddleware
    {
        private readonly RequestDelegate _next;
        public InterceptionServiceProviderMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task Invoke(HttpContext context)
        {
            IServiceProvider original = context.RequestServices;
            try
            {
                context.RequestServices = new InterceptableServiceProvider(context.RequestServices);
                await _next(context);
            }
            finally
            {
                context.RequestServices = original;
            }
        }
    }
}
