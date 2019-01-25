using Dora.GraphQL.Schemas;
using Dora.GraphQL.Server;
using GraphQL.Types;
using System;

namespace Dora.GraphQL.Server
{
    public interface IGraphSchemaConverter
    {
       ISchema Convert(IGraphSchema graphSchema);
    }
}
