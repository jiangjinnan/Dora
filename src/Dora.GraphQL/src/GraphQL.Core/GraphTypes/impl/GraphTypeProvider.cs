using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Dora.GraphQL.GraphTypes
{
    public class GraphTypeProvider : IGraphTypeProvider
    {
        private readonly IAttributeAccessor _attributeAccessor;
        private readonly ConcurrentDictionary<string, IGraphType> _graphTypes;

        public GraphTypeProvider(IAttributeAccessor attributeAccessor)
        {
            _attributeAccessor = attributeAccessor ?? throw new ArgumentNullException(nameof(attributeAccessor));
            _graphTypes = new ConcurrentDictionary<string, IGraphType>();

            IGraphType graphType;
            foreach (var type in GraphValueResolver.ScalarTypes)
            {
                graphType = new GraphType(_attributeAccessor, type, false, false);
                _graphTypes.TryAdd(graphType.Name, graphType);

                graphType = new GraphType(_attributeAccessor, type, true, false);
                _graphTypes.TryAdd(graphType.Name, graphType);

                graphType = new GraphType(_attributeAccessor, type, false, true);
                _graphTypes.TryAdd(graphType.Name, graphType);

                graphType = new GraphType(_attributeAccessor, type, true, true);
                _graphTypes.TryAdd(graphType.Name, graphType);
            }
        }

        public IGraphType GetGraphType(Type type, bool? isRequired, bool? isEnumerable)
        {
            Guard.ArgumentNotNull(type, nameof(type));
            type = GetValidType(type);
            var graphType = new GraphType(_attributeAccessor, type, isRequired, isEnumerable);
            _graphTypes.TryAdd(graphType.Name, graphType);
            return graphType;
        }

        public bool TryGetGraphType(string name, out IGraphType graphType)
        {
            Guard.ArgumentNotNullOrWhiteSpace(name, nameof(name));
            return _graphTypes.TryGetValue(name, out graphType);
        }

        private static Type GetValidType(Type type)
        {
            if (type == typeof(Task) && type == typeof(void) && type == typeof(ValueTask))
            {
                throw new GraphException("GraphType cannot be created based on type Task, ValueTask or void");
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>))
            {
                return type.GenericTypeArguments[0];
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ValueTask<>))
            {
                return type.GenericTypeArguments[0];
            }

            return type;
        }
    }
}
