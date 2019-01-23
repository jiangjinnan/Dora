using Dora.GraphQL.Executors;
using Dora.GraphQL.Options;
using Dora.GraphQL.Schemas;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
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
        private readonly GraphOptions   _graphOptions;
        private readonly GraphServerOptions  _serverOptions;
        private readonly IGraphSchemaFormatter _schemaFormatter;
        private readonly IGraphSchemaProvider  _schemaProvider;

        public GraphQLServerMiddleware(
            RequestDelegate next, 
            IGraphContextFactory graphContextFactory, 
            IGraphExecutor executor,
            IGraphSchemaFormatter schemaFormatter,
            IGraphSchemaProvider schemaProvider,
            IOptions<GraphOptions> graphOptionsAccessor,
            IOptions<GraphServerOptions> serverOptionsAccessor)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _graphContextFactory = graphContextFactory ?? throw new ArgumentNullException(nameof(graphContextFactory));
            _executor = executor ?? throw new ArgumentNullException(nameof(executor));
            _schemaFormatter = schemaFormatter ?? throw new ArgumentNullException(nameof(schemaFormatter));
            _schemaProvider = schemaProvider ?? throw new ArgumentNullException(nameof(schemaProvider));
            _graphOptions = (graphOptionsAccessor ?? throw new ArgumentNullException(nameof(graphOptionsAccessor))).Value;
            _serverOptions = (serverOptionsAccessor ?? throw new ArgumentNullException(nameof(serverOptionsAccessor))).Value;
        }

        public async Task InvokeAsync(HttpContext httpContext, IHostingEnvironment hostingEnvironment)
        {
            if (!httpContext.Request.Path.StartsWithSegments(_serverOptions.PathBase))
            {
                await _next(httpContext);
                return;
            }

            if (httpContext.Request.Path.StartsWithSegments(_serverOptions.PathBase.Add("/schema")) && httpContext.Request.Method == "GET")
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
                        <pre>" +
                        _schemaFormatter.Format(_schemaProvider.GetSchema(), format) + 
                        @"</pre>
                    </body>
                </html>");
                return;
            }

            if (httpContext.Request.Method != "POST")
            {
                await _next(httpContext);
                return;
            }

            var payload = Deserialize<RequestPayload>(httpContext.Request.Body);
            var context = await _graphContextFactory.CreateAsync(payload);
            var result = await _executor.ExecuteAsync(context);
            httpContext.Response.ContentType = "application/json";

            if (_graphOptions.FieldNamingConvention == FieldNamingConvention.PascalCase)
            {
                await httpContext.Response.WriteAsync(JsonConvert.SerializeObject(result.Data, new StringEnumConverter()));
            }
            else
            {
                var settings = new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    Converters = new List<JsonConverter> { new StringEnumConverter() }
                };
                await httpContext.Response.WriteAsync(JsonConvert.SerializeObject(result.Data, Formatting.None, settings));
            }

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
