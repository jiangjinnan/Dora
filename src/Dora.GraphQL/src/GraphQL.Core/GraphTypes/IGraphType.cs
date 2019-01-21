using System;
using System.Collections.Generic;

namespace Dora.GraphQL.GraphTypes
{
    public interface IGraphType
    {
        Type Type { get; }
        //For Union Type
        Type[] OtherTypes { get; }
        string Name { get; }
        bool IsRequired { get; }
        bool IsEnumerable { get; }
        bool IsEnum { get; }
        IDictionary<NamedType, GraphField> Fields { get; }
        object Resolve(object rawValue);
    }
}
