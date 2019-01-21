using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Dora.GraphQL.Schemas;
using Microsoft.Extensions.Hosting;

namespace Dora.GraphQL.Server
{
    public class GraphSchemaProvider : IGraphSchemaProvider
    {
        private readonly Lazy<IGraphSchema> _schemaAccessor;

        public GraphSchemaProvider(ISchemaFactory schemaFactory, IHostingEnvironment environment)
        {
            Guard.ArgumentNotNull(schemaFactory, nameof(schemaFactory));
            Guard.ArgumentNotNull(environment, nameof(environment));
            _schemaAccessor = new Lazy<IGraphSchema>(() =>
            {
                var assemblyName = new AssemblyName(environment.ApplicationName);
                return schemaFactory.Create(Assembly.Load(assemblyName));
            });

        }
        public IGraphSchema GetSchema() => _schemaAccessor.Value;
    }
}
