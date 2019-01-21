using Dora.GraphQL.Executors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Dora.GraphQL.Server
{
    public class GraphQLServerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IGraphContextFactory _graphContextFactory;
        private readonly IGraphExecutor _executor;
        private readonly GraphServerOptions  _options;

        public GraphQLServerMiddleware(
            RequestDelegate next, 
            IGraphContextFactory graphContextFactory, 
            IGraphExecutor executor,
            IOptions<GraphServerOptions> optionsAccessor)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _graphContextFactory = graphContextFactory ?? throw new ArgumentNullException(nameof(graphContextFactory));
            _executor = executor ?? throw new ArgumentNullException(nameof(executor));
            _options = (optionsAccessor ?? throw new ArgumentNullException(nameof(optionsAccessor))).Value;
        }

        public async Task InvokeAsync(HttpContext httpContext, IHostingEnvironment hostingEnvironment)
        {
            if (httpContext.Request.Path != _options.PathBase)
            {
                await _next(httpContext);
                return;
            }

            var payload = Deserialize<RequestPayload>(httpContext.Request.Body);
            var context = await _graphContextFactory.CreateAsync(payload);
            var result = await _executor.ExecuteAsync(context);
            httpContext.Response.ContentType = "application/json";
            await httpContext.Response.WriteAsync(JsonConvert.SerializeObject(result.Data, new StringEnumConverter()));
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
