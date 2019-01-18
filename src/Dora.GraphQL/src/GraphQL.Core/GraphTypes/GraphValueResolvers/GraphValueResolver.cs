using System;

namespace Dora.GraphQL.GraphTypes
{
    public partial class GraphValueResolver
    {   
        public string Name { get; }
        public Type Type { get; }
        public Func<object, object> ValueResolver { get; }
        public bool IsScalar { get; }
        public GraphValueResolver(string name, Type type, bool isScalar, Func<object, object> valueResolver)
        {
            Name = Guard.ArgumentNotNullOrWhiteSpace(name, nameof(name));
            Type = type ?? throw new ArgumentNullException(nameof(type));
            IsScalar = isScalar;
            ValueResolver = valueResolver ?? throw new ArgumentNullException(nameof(valueResolver));
        }
    }
}
