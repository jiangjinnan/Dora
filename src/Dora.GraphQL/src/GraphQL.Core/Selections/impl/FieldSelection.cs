using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Dora.GraphQL.Selections.impl
{
    public class FieldSelection : IFieldSelection
    {
        public string Name { get; }
        public string Alias { get; set; }
        public IDictionary<string, NamedValueToken> Arguments { get; }
        public IDictionary<string, IDirective> Directives { get; }
        public ICollection<ISelectionNode> SelectionSet { get; }
        public FieldSelection(string name)
        {
            Name = Guard.ArgumentNotNullOrWhiteSpace( name, nameof(name));
            Arguments = new Dictionary<string, NamedValueToken>();
            Directives = new Dictionary<string, IDirective>();
            SelectionSet = new Collection<ISelectionNode>();
        }
    }
}
