using Dora.GraphQL.GraphTypes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dora.GraphQL.Selections
{
    public interface IDirective
    {
        string Name { get; }
        IDictionary<string, NamedValueToken> Arguments { get; }
    }
}
