using Dora.GraphQL.Executors;
using Dora.GraphQL.Schemas;
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
        private readonly IGraphSchemaFormatter _schemaFormatter;
        private readonly IGraphSchemaProvider  _schemaProvider;

        public GraphQLServerMiddleware(
            RequestDelegate next, 
            IGraphContextFactory graphContextFactory, 
            IGraphExecutor executor,
            IGraphSchemaFormatter schemaFormatter,
            IGraphSchemaProvider schemaProvider,
            IOptions<GraphServerOptions> optionsAccessor)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _graphContextFactory = graphContextFactory ?? throw new ArgumentNullException(nameof(graphContextFactory));
            _executor = executor ?? throw new ArgumentNullException(nameof(executor));
            _schemaFormatter = schemaFormatter ?? throw new ArgumentNullException(nameof(schemaFormatter));
            _schemaProvider = schemaProvider ?? throw new ArgumentNullException(nameof(schemaProvider));
            _options = (optionsAccessor ?? throw new ArgumentNullException(nameof(optionsAccessor))).Value;
        }

        public async Task InvokeAsync(HttpContext httpContext, IHostingEnvironment hostingEnvironment)
        {
            if (!httpContext.Request.Path.StartsWithSegments(_options.PathBase))
            {
                await _next(httpContext);
                return;
            }

            if (httpContext.Request.Path.StartsWithSegments(_options.PathBase.Add("/schema")))
            {
                var format = httpContext.Request.Query.ContainsKey("inline")
                    ? GraphSchemaFormat.Inline
                    : GraphSchemaFormat.GQL;

                httpContext.Response.ContentType = "text/html";
                await httpContext.Response.WriteAsync(@"
                <html>
                    <head>
                        <title>Graph Schema</title>
                    </head>
                    <body>
                        <pre>
                        " + _schemaFormatter.Format(_schemaProvider.GetSchema(), format) + @"
                        </pre>
                    </body>
                </html>");
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
