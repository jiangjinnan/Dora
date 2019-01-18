using System;

namespace Dora.GraphQL.Schemas
{
    [AttributeUsage( AttributeTargets.Property)]
    public sealed class GraphMemberAttribute: Attribute
    {
        public string Name { get; set; }
        public bool Ignored { get; set; }
        public bool IsRequired { get; set; }
        public string Resolver { get; set; }
    }
}
