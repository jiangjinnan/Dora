using Dora.GraphQL.ArgumentBinders;
using Dora.GraphQL.Descriptors;
using Dora.GraphQL.GraphTypes;
using Dora.GraphQL.Resolvers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dora.GraphQL.Schemas
{
    /// <summary>
    /// Default implementation of <see cref="ISchemaFactory"/>.
    /// </summary>
    /// <seealso cref="Dora.GraphQL.Schemas.ISchemaFactory" />
    public class SchemaFactory : ISchemaFactory
    {
        #region Fields
        private readonly IAttributeAccessor _attributeAccessor;
        private readonly IGraphTypeProvider _graphTypeProvider;
        private readonly IArgumentBinderProvider _binderProvider;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaFactory"/> class.
        /// </summary>
        /// <param name="attributeAccessor">The attribute accessor.</param>
        /// <param name="graphTypeProvider">The graph type provider.</param>
        /// <param name="binderProvider">The binder provider.</param>
        public SchemaFactory(
            IAttributeAccessor attributeAccessor, 
            IGraphTypeProvider graphTypeProvider,
            IArgumentBinderProvider binderProvider)
        {
            _attributeAccessor = attributeAccessor ?? throw new ArgumentNullException(nameof(attributeAccessor));
            _graphTypeProvider = graphTypeProvider ?? throw new ArgumentNullException(nameof(graphTypeProvider));
            _binderProvider = binderProvider ?? throw new ArgumentNullException(nameof(binderProvider));
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Creates the GraphQL schema factory based on specified GraphQL services.
        /// </summary>
        /// <param name="services">The <see cref="T:Dora.GraphQL.Descriptors.GraphServiceDescriptor" /> list.</param>
        /// <returns>
        /// The <see cref="T:Dora.GraphQL.Schemas.IGraphSchema" />.
        /// </returns>
        public IGraphSchema Create(IEnumerable<GraphServiceDescriptor> services)
        {           
            var knownTypes = services.SelectMany(it => it.KnownTypes).ToList();
            var methods = services.SelectMany(it => it.Operations.Values).ToArray();
            knownTypes.AddRange(methods.SelectMany(it => it.KnownTypes));
            foreach (var knowType in knownTypes)
            {
                _graphTypeProvider.GetGraphType(knowType, false, false);
            }

            var methodGroups = methods
                .GroupBy(it => it.OperationType, it => it)
                .ToDictionary(it => it.Key, it => it);
            
            var query = CreateGraphType(OperationType.Query, methodGroups.TryGetValue(OperationType.Query, out var queryMethods) ? queryMethods : null);
            var mutation = CreateGraphType(OperationType.Mutation, methodGroups.TryGetValue(OperationType.Mutation, out var mutationMethods) ? mutationMethods : null);
            var subscription = CreateGraphType(OperationType.Subscription, methodGroups.TryGetValue(OperationType.Subscription, out var subscirptionMethods) ? subscirptionMethods : null);
            var schema = new GraphSchema(query, mutation, subscription);
            foreach (var field in schema.Fields.Values)
            {
                field.SetHasCustomResolverFlags();
            }
            return schema;
        }
        #endregion

        #region Private methods
        private IGraphType CreateGraphType(OperationType operationType, IEnumerable<GraphOperationDescriptor> operations)
        {
            var graphType = new GraphType(operationType);
            if (operations == null)
            {
                return graphType;
            }
            foreach (var operation in operations)
            {
                var resolver = new OperationResolver(operation, _binderProvider);
                var type = _graphTypeProvider.GetGraphType(operation.MethodInfo.ReturnType, null, null);

                var field = new GraphField(operation.Name, type, typeof(void), resolver);
                foreach (var parameter in operation.Parameters.Values)
                {
                    if (!parameter.IsGraphArgument)
                    {
                        continue;
                    }
                    var parameterGraphType = _graphTypeProvider.GetGraphType(parameter.ParameterInfo.ParameterType, parameter.IsRequired, null);
                    field.AddArgument(new NamedGraphType(parameter.ArgumentName, parameterGraphType));
                }

                graphType.AddField(typeof(void), field);
            }
            return graphType;
        }
        #endregion
    }
}