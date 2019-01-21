using Dora.GraphQL.Executors;
using Dora.GraphQL.Selections;

namespace Dora.GraphQL.GraphTypes
{
    public struct ResolverContext
    {       
        public GraphContext GraphContext { get; }
        public GraphField  Field { get; }
        public IFieldSelection Selection { get; }
        public object Container { get; }
        public ResolverContext(GraphContext graphContext, GraphField  field, IFieldSelection selection,  object container)
        {
            Container = container;
            GraphContext = Guard.ArgumentNotNull(graphContext, nameof(graphContext));
            Field = Guard.ArgumentNotNull(field, nameof(field));
            Selection = Guard.ArgumentNotNull(selection, nameof(selection));
        }
    }
}
