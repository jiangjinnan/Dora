using Dora.GraphQL.GraphTypes;
using System.Reflection;

namespace Dora.GraphQL.Schemas
{
    public interface ISchemaFactory
    {
        IGraphSchema Create(Assembly assembly);
    }
}
