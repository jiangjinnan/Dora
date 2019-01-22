using Dora.GraphQL.GraphTypes;
using Dora.GraphQL.Resolvers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dora.GraphQL.Schemas
{
    public sealed class GraphSchema : IGraphSchema
    {
        public GraphSchema(IGraphType query, IGraphType mutation, IGraphType subsription)
        {
            Query = query ?? throw new ArgumentNullException(nameof(query));
            Mutation = mutation ?? throw new ArgumentNullException(nameof(mutation));
            Subsription = subsription ?? throw new ArgumentNullException(nameof(subsription));
            Fields = new Dictionary<NamedType, GraphField>();

            var @void = typeof(void);
            var resolver = FakeResolver.Instance;

            var queryField = new GraphField(GraphDefaults.GraphSchema.QueryFieldName, query, @void, resolver);
            var mutationField = new GraphField(GraphDefaults.GraphSchema.MutationFieldName, mutation, @void, resolver);
            var subscriptionField = new GraphField(GraphDefaults.GraphSchema.SubscriptionFieldName, subsription, @void, resolver);
            Fields.Add(new NamedType(GraphDefaults.GraphSchema.QueryFieldName, @void), queryField);
            Fields.Add(new NamedType(GraphDefaults.GraphSchema.MutationFieldName, @void), mutationField);
            Fields.Add(new NamedType(GraphDefaults.GraphSchema.SubscriptionFieldName, @void), subscriptionField);
        }

        public IGraphType Query { get; }
        public IGraphType Mutation { get; }
        public IGraphType Subsription { get; }

        public Type Type => typeof(void);

        public Type[] OtherTypes => Type.EmptyTypes;

        public string Name => GraphDefaults.GraphSchema.GraphTypeName;

        public bool IsRequired => true;

        public bool IsEnumerable => false;

        public bool IsEnum => false;

        public IDictionary<NamedType, GraphField> Fields { get; }

        public object Resolve(object rawValue) => null;

        
    }
}
