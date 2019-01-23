using Dora.GraphQL.GraphTypes;

namespace Dora.GraphQL.Schemas
{
    public interface IGraphSchema: IGraphType
    {
        IGraphType Query { get; }
        IGraphType Mutation { get; }
        IGraphType Subsription { get; }
    }
}
