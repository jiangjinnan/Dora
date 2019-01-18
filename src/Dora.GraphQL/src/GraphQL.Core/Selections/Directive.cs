using System.Collections.Generic;

namespace Dora.GraphQL.Selections
{
    public interface IDirective
    {
        string Name { get; }
        IDictionary<string, NamedValueToken> Arguments { get; }
    }
}
