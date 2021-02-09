using Dora.Interception;
using Microsoft.AspNetCore.Http;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace WebApp
{
    public class TraceElapsedAttribute: InterceptorAttribute
    {
        //public bool CaptureArguments => true;
        public async Task InvokeAsync(InvocationContext invocationContext, IHttpContextAccessor httpContextAccessor)
        {
            var stopwatch = Stopwatch.StartNew();
            await invocationContext.InvokeAsync();
            stopwatch.Stop();
            await httpContextAccessor.HttpContext.Response.WriteAsync($"[TraceElapsed]{invocationContext.Target.GetType()}.{invocationContext.Method.Name}:{stopwatch.Elapsed}" + Environment.NewLine + Environment.NewLine);
        }
        protected override object CreateInterceptor(IServiceProvider serviceProvider) => this;
    }
}