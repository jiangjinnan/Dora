using Dora.Interception;
using Microsoft.AspNetCore.Http;
using System;
using System.Text;
using System.Threading.Tasks;

namespace WebApp
{
    [AttributeUsage(AttributeTargets.Method)]
    public class TraceInputsAttribute: InterceptorAttribute
    {
        public bool CaptureArguments => true;
        public async Task InvokeAsync(InvocationContext invocationContext, IHttpContextAccessor httpContextAccessor)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"[TraceInputs]{invocationContext.Target.GetType()}.{invocationContext.Method.Name}({string.Join(",", invocationContext.Arguments)})");          
            await httpContextAccessor.HttpContext.Response.WriteAsync(builder.ToString());
            await invocationContext.InvokeAsync();
        }
        protected override object CreateInterceptor(IServiceProvider serviceProvider) => this;
    }
}
