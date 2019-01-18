using Dora.GraphQL.GraphTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.GraphQL
{
    public static class GraphTypeExtensions
    {
        public static IGraphType AddField(this IGraphType graphType, GraphField graphField)
        {
            Guard.ArgumentNotNull(graphType, nameof(graphType));
            Guard.ArgumentNotNull(graphField, nameof(graphField));
            graphType.Fields.Add(graphField.Name, graphField);
            return graphType;
        }
    }
}
