using System.Collections.Generic;

namespace Dora.GraphQL.Selections
{
    public interface IFieldSelection :ISelectionNode
    {
        string Name { get; }
        string Alias { get; }
        IDictionary<string, NamedValueToken> Arguments { get; }
        IDictionary<string, IDirective> Directives { get; }
    }
}