using Dora.GraphQL.Schemas;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.GraphQL.Server
{
    public interface IGraphSchemaProvider
    {
        IGraphSchema GetSchema();
    }
}
