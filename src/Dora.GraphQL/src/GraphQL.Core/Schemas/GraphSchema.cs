using Dora.GraphQL.GraphTypes;
using Dora.GraphQL.Resolvers;
using System;
using System.Collections.Generic;

namespace Dora.GraphQL.Schemas
{
    /// <summary>
    /// Default implementation of <see cref="IGraphSchema"/>.
    /// </summary>
    public sealed class GraphSchema : IGraphSchema
    {
        /// <summary>
        /// Gets the query specific <see cref="T:Dora.GraphQL.GraphTypes.IGraphType" />.
        /// </summary>
        /// <value>
        /// The query specific <see cref="T:Dora.GraphQL.GraphTypes.IGraphType" />.
        /// </value>
        public IGraphType Query { get; }

        /// <summary>
        /// Gets the mutation specific <see cref="T:Dora.GraphQL.GraphTypes.IGraphType" />.
        /// </summary>
        /// <value>
        /// The mutation specific <see cref="T:Dora.GraphQL.GraphTypes.IGraphType" />.
        /// </value>
        public IGraphType Mutation { get; }

        /// <summary>
        /// Gets the subsription specific <see cref="T:Dora.GraphQL.GraphTypes.IGraphType" />.
        /// </summary>
        /// <value>
        /// The subsription specific <see cref="T:Dora.GraphQL.GraphTypes.IGraphType" />.
        /// </value>
        public IGraphType Subscription { get; }

        /// <summary>
        /// Gets the CLR type.
        /// </summary>
        /// <value>
        /// The CLR type.
        /// </value>
        public Type Type => typeof(void);

        /// <summary>
        /// Gets the other types for union type.
        /// </summary>
        /// <value>
        /// The other types for union type.
        /// </value>
        /// <remarks>
        /// It is an empty array for non union type.
        /// </remarks>
        public Type[] OtherTypes => Type.EmptyTypes;

        /// <summary>
        /// Gets the GraphQL type name.
        /// </summary>
        /// <value>
        /// The  GraphQL type name.
        /// </value>
        public string Name => GraphDefaults.GraphSchema.GraphTypeName;

        /// <summary>
        /// Gets a value indicating whether this is a required (non-optional) GraphQL type.
        /// </summary>
        /// <value>
        /// <c>true</c> if this is required; otherwise, <c>false</c>.
        /// </value>
        public bool IsRequired => true;

        /// <summary>
        /// Gets a value indicating whether this is an enumerable (array) GraphQL type.
        /// </summary>
        /// <value>
        /// <c>true</c> if this is enumerable; otherwise, <c>false</c>.
        /// </value>
        public bool IsEnumerable => false;

        /// <summary>
        /// Gets a value indicating whether this is an enum GraphQL type.
        /// </summary>
        /// <value>
        /// <c>true</c> if this is enum; otherwise, <c>false</c>.
        /// </value>
        public bool IsEnum => false;

        /// <summary>
        /// Gets the fields.
        /// </summary>
        /// <value>
        /// The fields.
        /// </value>
        /// <remarks>
        /// The key is field name + container CLR type. For union GraphQL types, all member types' fields are included.
        /// </remarks>
        public IDictionary<NamedType, GraphField> Fields { get; }

        /// <summary>
        /// Resolves the value based on provided raw value.
        /// </summary>
        /// <param name="rawValue">The raw value.</param>
        /// <returns>
        /// The real value.
        /// </returns>
        public object Resolve(object rawValue) => null;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphSchema"/> class.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="mutation">The mutation.</param>
        /// <param name="subsription">The subsription.</param>
        public GraphSchema(IGraphType query, IGraphType mutation, IGraphType subsription)
        {
            Query = query ?? throw new ArgumentNullException(nameof(query));
            Mutation = mutation ?? throw new ArgumentNullException(nameof(mutation));
            Subscription = subsription ?? throw new ArgumentNullException(nameof(subsription));
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
    }
}
