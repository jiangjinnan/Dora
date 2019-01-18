using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Dora.GraphQL.Selections.impl
{
    public class SelectionNode : ISelectionNode
    {
        public string Name { get; }
        public string Alias { get; set; }
        public IDictionary<string, NamedValueToken> Arguments { get; }
        public IDictionary<string, IDirective> Directives { get; }
        public string Fragment { get; set; }
        public IDictionary<string, ISelectionNode> Children { get; }
        public SelectionNode(string name)
        {
            Name = Guard.ArgumentNotNullOrWhiteSpace( name, nameof(name));
            Arguments = new Dictionary<string, NamedValueToken>();
            Directives = new Dictionary<string, IDirective>();
            Children = new Dictionary<string, ISelectionNode>();
        }
    }
}
