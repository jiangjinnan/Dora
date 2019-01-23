using Dora.GraphQL.Schemas;

namespace Dora.GraphQL.Server
{
    public interface IGraphSchemaProvider
    {
        IGraphSchema Schema { get; }
    }
}
