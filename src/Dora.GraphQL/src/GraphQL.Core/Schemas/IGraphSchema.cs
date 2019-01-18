using Dora.GraphQL.GraphTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.GraphQL.Schemas
{
    public interface IGraphSchema
    {
        IGraphType Query { get; }
        IGraphType Mutation { get; }
        IGraphType Subsription { get; }
    }
}
