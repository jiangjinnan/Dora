using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp
{
    public interface IWriter
    {
        ValueTask WriteLineAsync<T>(T data);
    }
    public class Writer : IWriter
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public Writer(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        //[TraceInputs(Order = 1)]
        [TraceElapsed(Order = 2)]
        public virtual async ValueTask WriteLineAsync<T>(T data)
        {
            var stopwatch = Stopwatch.StartNew();
            await Task.Delay(50);
            stopwatch.Stop();
            await _httpContextAccessor.HttpContext.Response.WriteAsync($"Real elapsed: {stopwatch.Elapsed}" + Environment.NewLine);
        }
    }
}
