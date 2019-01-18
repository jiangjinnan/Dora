using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.GraphQL.GraphTypes.impl
{
    public class GraphValueResolverProvider : IGraphValueResolverProvider
    {
        private readonly Dictionary<Type, GraphValueResolver> _graphValueResolvers;
        public GraphValueResolverProvider()
        {
            _graphValueResolvers = new Dictionary<Type, GraphValueResolver>
            {
                [typeof(bool)] = GraphValueResolver.Boolean,
                [typeof(DateTime)] = GraphValueResolver.DateTime,
                [typeof(DateTimeOffset)] = GraphValueResolver.DateTimeOffset,
                [typeof(decimal)] = GraphValueResolver.Decimal,
                [typeof(float)] = GraphValueResolver.Float,
                [typeof(int)] = GraphValueResolver.Int,
                [typeof(long)] = GraphValueResolver.Long,
                [typeof(TimeSpan)] = GraphValueResolver.TimeSpan,
            };
        }
        public GraphValueResolver GetGraphTypeResolver(Type type)
        => _graphValueResolvers.TryGetValue(type, out var value)
            ? value
            : GraphValueResolver.Complex(type);
    }
}
