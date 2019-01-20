using System;

namespace Dora.GraphQL.GraphTypes
{
    public  struct NamedGraphType: IEquatable<NamedGraphType>
    {        
        public string Name { get; }
        public IGraphType GraphType { get; }
        public object DefaultValue { get; }
        public NamedGraphType(string name, IGraphType graphType, object defaultValue = null):this()
        {
            Name = Guard.ArgumentNotNullOrWhiteSpace(name, nameof(name));
            GraphType = Guard.ArgumentNotNull(graphType, nameof(graphType));
            DefaultValue = defaultValue;
        }

        public bool Equals(NamedGraphType other)
        {
            return Name == other.Name && GraphType.Name == other.GraphType.Name && DefaultValue == other.DefaultValue;
        }

        public override int GetHashCode() => $"{Name}.{GraphType.Name}.{DefaultValue ?? ""}".GetHashCode();
    }
}
