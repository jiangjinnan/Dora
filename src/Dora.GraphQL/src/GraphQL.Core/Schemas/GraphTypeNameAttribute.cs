using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.GraphQL.Schemas
{
    [AttributeUsage( AttributeTargets.Class| AttributeTargets.Struct)]
    public class GraphTypeNameAttribute: Attribute
    {
        public GraphTypeNameAttribute(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }
        public string Name { get; }
    }
}
