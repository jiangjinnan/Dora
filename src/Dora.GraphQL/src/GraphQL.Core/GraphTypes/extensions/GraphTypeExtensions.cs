using Dora.GraphQL.GraphTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.GraphQL
{
    public static class GraphTypeExtensions
    {
        public static IGraphType AddField(this IGraphType graphType, Type containerType, GraphField graphField)
        {
            Guard.ArgumentNotNull(graphType, nameof(graphType));
            Guard.ArgumentNotNull(containerType, nameof(containerType));
            Guard.ArgumentNotNull(graphField, nameof(graphField));
            graphType.Fields.Add(new NamedType(graphField.Name, containerType), graphField);
            return graphType;
        }
    }
}
