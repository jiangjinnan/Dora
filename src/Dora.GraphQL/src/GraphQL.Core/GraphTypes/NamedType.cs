using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.GraphQL.GraphTypes
{
    public struct NamedType:IEquatable<NamedType>
    {
        public NamedType(string name, Type type) : this()
        {
            Name = Guard.ArgumentNotNullOrWhiteSpace(name, nameof(name));
            Type = type ?? throw new ArgumentNullException(nameof(type));
        }

        public string Name { get; }
        public Type Type { get; }
        public bool Equals(NamedType other) => Name == other.Name && Type == other.Type;
        public override int GetHashCode() => $"{Name}.{Type.AssemblyQualifiedName}".GetHashCode();
    }
}
