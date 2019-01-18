using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.GraphQL.GraphTypes
{
    public interface IGraphType
    {
        Type Type { get; }
        string Name { get; }
        bool IsRequired { get; }
        bool IsEnumerable { get; }
        IDictionary<string, GraphField> Fields { get; }
        object Resolve(object rawValue);
    }
}
