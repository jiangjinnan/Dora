using Demo1;
using Dora.GraphQL;
using Dora.GraphQL.ArgumentBinders;
using Dora.GraphQL.Descriptors;
using Dora.GraphQL.GraphTypes;
using Dora.GraphQL.Schemas;
using Dora.GraphQL.Server;
using GraphQL.Samples.Schemas.Chat;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using Xunit;

namespace GraphQL.Test
{
    public class SchemaConverter
    {
        [Fact]
        public void Convert()
        {
            //var chat = new ChatSchema(new Chat());
            //var types = chat.AllTypes;

            var service = new GraphServiceDescriptor(typeof(DemoGraphService));
            var serviceProvider = new ServiceCollection().BuildServiceProvider();
            var graphTypeProvider = new GraphTypeProvider(serviceProvider, new AttributeAccessor(), Options.Create(new GraphOptions()));
            var factory = new SchemaFactory(new AttributeAccessor(), graphTypeProvider, new ArgumentBinderProvider(new IArgumentBinder[] { new GraphArgumentBinder() }));
            var schema = factory.Create(new GraphServiceDescriptor[] { service });

            var converter = new GraphSchemaConverter(graphTypeProvider);
            var convertedSchema = converter.Convert(schema);
            var types = convertedSchema.AllTypes;
        }
    }
}
