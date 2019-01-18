using System.Collections.Generic;

namespace Dora.GraphQL.GraphTypes
{
    public class GraphField
    {
        public string Name { get; }
        public IGraphType GraphType { get; }
        public IGraphResolver Resolver { get; }
        public IDictionary<string, NamedGraphType> Arguments { get; }
        public GraphField(string name, IGraphType graphType, IGraphResolver resolver)
        {
            Name = Guard.ArgumentNotNullOrWhiteSpace(name, nameof(graphType));
            GraphType = Guard.ArgumentNotNull( graphType, nameof(graphType));
            Resolver = Guard.ArgumentNotNull( resolver, nameof(resolver));
            Arguments = new Dictionary<string, NamedGraphType>();
        }
        public GraphField AddArgument(NamedGraphType argument)
        {
            Arguments[argument.Name] = argument;
            return this;
        }
    }
}
