using System;

namespace Dora.GraphQL.GraphTypes
{
    public interface IGraphTypeProvider
    {
        IGraphType GetGraphType(Type type, bool? isRequired, bool? isEnumerable);
        bool TryGetGraphType(string name, out IGraphType graphType);
    }
}
