using Dora.GraphQL.Executors;
using Dora.GraphQL.Schemas;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
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
        private readonly ILogger _logger;
        private readonly Action<ILogger, DateTimeOffset, string, string, Exception> _log4Error;

        public GraphQLServerMiddleware(
            RequestDelegate next,
            IGraphContextFactory graphContextFactory,
            IGraphExecutor executor,
            IGraphSchemaFormatter schemaFormatter,
            IGraphSchemaProvider schemaProvider,
            IOptions<GraphOptions> graphOptionsAccessor,
            IOptions<GraphServerOptions> serverOptionsAccessor,
            ILogger<GraphQLServerMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _graphContextFactory = graphContextFactory ?? throw new ArgumentNullException(nameof(graphContextFactory));
            _executor = executor ?? throw new ArgumentNullException(nameof(executor));
            _schemaFormatter = schemaFormatter ?? throw new ArgumentNullException(nameof(schemaFormatter));
            _schemaProvider = schemaProvider ?? throw new ArgumentNullException(nameof(schemaProvider));
            _graphOptions = (graphOptionsAccessor ?? throw new ArgumentNullException(nameof(graphOptionsAccessor))).Value;
            _serverOptions = (serverOptionsAccessor ?? throw new ArgumentNullException(nameof(serverOptionsAccessor))).Value;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _log4Error = LoggerMessage.Define<DateTimeOffset, string, string>(LogLevel.Error, 0, "[{0}]Unhandled exception. Operation: {1}. Detailed information: {2}");
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
                await HandleSchemaRequest(httpContext);
                return;
            }

            if (httpContext.Request.Method != "POST")
            {
                httpContext.Response.StatusCode = 405;
                await httpContext.Response.WriteAsync("The HTTP method of GraphQL request must be POST.");
                return;
            }

            var contentType = httpContext.Request.ContentType;
            if (contentType != "application/json" && httpContext.Response.ContentType != "text/json")
            {
                httpContext.Response.StatusCode = 406;
                await httpContext.Response.WriteAsync("The Content-Type of GraphQL request must be application/json.");
                return;
            }
            await HandleOperationRequest(httpContext);
        }

        private async Task HandleOperationRequest(HttpContext httpContext)
        {
            var payload = Deserialize<RequestPayload>(httpContext.Request.Body);
            try
            {
                var context = await _graphContextFactory.CreateAsync(payload);
                httpContext.Features.Set<IGraphContextFeature>(new GraphContextFeature(context));
                var result = await _executor.ExecuteAsync(context);
                httpContext.Response.ContentType = "application/json";

                if (_graphOptions.FieldNameConverter == FieldNameConverter.CamelCase)
                {
                    var settings = new JsonSerializerSettings
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver(),
                        Converters = new List<JsonConverter> { new StringEnumConverter() }
                    };
                    await httpContext.Response.WriteAsync(JsonConvert.SerializeObject(result, Formatting.None, settings));
                }
                else
                {
                    await httpContext.Response.WriteAsync(JsonConvert.SerializeObject(result, new StringEnumConverter()));
                }
            }
            catch (Exception ex)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                {
                    _log4Error(_logger, DateTimeOffset.Now, payload.OperationName, ErrorFormatter.Instance.Format(ex), null);
                }
                throw;
            }
        }

        private async Task HandleSchemaRequest(HttpContext httpContext)
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
                    _schemaFormatter.Format(_schemaProvider.Schema, format) +
                    @"</pre>
                    </body>
                </html>");
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
