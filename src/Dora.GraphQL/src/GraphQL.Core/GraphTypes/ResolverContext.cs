using Dora.GraphQL.Executors;
using Dora.GraphQL.Selections;
using System;
using System.Collections.Generic;

namespace Dora.GraphQL.GraphTypes
{
    public struct ResolverContext
    {       
        public GraphContext GraphContext { get; }
        public GraphField  Field { get; }
        public ISelectionNode Selection { get; }
        public object Container { get; }
        public ResolverContext(GraphContext graphContext, GraphField  field, ISelectionNode selection,  object container)
        {
            Container = container;
            GraphContext = Guard.ArgumentNotNull(graphContext, nameof(graphContext));
            Field = Guard.ArgumentNotNull(field, nameof(field));
            Selection = Guard.ArgumentNotNull(selection, nameof(selection));
        }
    }
}
