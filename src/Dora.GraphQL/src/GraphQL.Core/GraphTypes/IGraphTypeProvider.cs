using System;

namespace Dora.GraphQL.GraphTypes
{
    public interface IGraphTypeProvider
    {
        IGraphType GetGraphType(Type type, bool? isRequired, bool? isEnumerable, params Type[] otherTypes);
        bool TryGetGraphType(string name, out IGraphType graphType);
        IGraphTypeProvider AddGraphType(string name, IGraphType graphType);
    }
}
