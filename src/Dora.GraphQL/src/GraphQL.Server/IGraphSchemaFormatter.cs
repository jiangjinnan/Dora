using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.GraphQL.Schemas
{
    public interface IGraphSchemaFormatter
    {
        string Format(IGraphSchema schema, GraphSchemaFormat graphSchema);
    }

    public enum GraphSchemaFormat
    {
        GQL,
        Inline
    }
}
