using Dora.GraphQL.Executors;
using Dora.GraphQL.Schemas;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Dora.GraphQL.Server
{
    public class GraphMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IGraphContextFactory _graphContextFactory;
        private readonly IGraphExecutor _executor;

        public GraphMiddleware(RequestDelegate next, IGraphContextFactory graphContextFactory, IGraphExecutor executor)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _graphContextFactory = graphContextFactory ?? throw new ArgumentNullException(nameof(graphContextFactory));
            _executor = executor ?? throw new ArgumentNullException(nameof(executor));
        }

        public async Task InvokeAsync(HttpContext httpContext, IHostingEnvironment hostingEnvironment)
        {
            var sw = Stopwatch.StartNew();
            var payload = Deserialize<RequestPayload>(httpContext.Request.Body);
            var context = await _graphContextFactory.CreateAsync(payload);
            var result = await _executor.ExecuteAsync(context);
            httpContext.Response.ContentType = "application/json";
            await httpContext.Response.WriteAsync(JsonConvert.SerializeObject(result.Data));
        }
        private static T Deserialize<T>(Stream s)
        {
            using (var reader = new StreamReader(s))
            using (var jsonReader = new JsonTextReader(reader))
            {
                return new JsonSerializer().Deserialize<T>(jsonReader);
            }
        }
    }
}
