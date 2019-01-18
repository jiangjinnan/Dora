using System.Collections.Generic;

namespace Dora.GraphQL.Selections
{
    public interface ISelectionNode
    {
        string Name { get; }
        string Alias { get; }
        IDictionary<string, NamedValueToken> Arguments { get; }
        IDictionary<string, IDirective> Directives { get; }
        string Fragment { get; }
        IDictionary<string, ISelectionNode> Children { get; }
    }
}