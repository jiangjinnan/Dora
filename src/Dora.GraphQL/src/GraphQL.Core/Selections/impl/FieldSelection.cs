using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Dora.GraphQL.Selections
{
    public class FieldSelection : IFieldSelection
    {
        public string Name { get; }
        public string Alias { get; set; }
        public IDictionary<string, NamedValueToken> Arguments { get; }
        public ICollection<IDirective> Directives { get; }
        public ICollection<ISelectionNode> SelectionSet { get; }
        public IDictionary<string, object> Properties { get; }
        public FieldSelection(string name)
        {
            Name = Guard.ArgumentNotNullOrWhiteSpace( name, nameof(name));
            Arguments = new Dictionary<string, NamedValueToken>();
            Directives = new Collection<IDirective>();
            SelectionSet = new Collection<ISelectionNode>();
            Properties = new Dictionary<string, object>();
        }
    }
}
