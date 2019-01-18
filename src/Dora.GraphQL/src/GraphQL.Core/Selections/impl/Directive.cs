using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Dora.GraphQL.Selections.impl
{
    public class Directive: IDirective
    {
        public string Name { get; }
        public IDictionary<string, NamedValueToken> Arguments  { get; }

        public Directive(string name)
        {
            Name = Guard.ArgumentNotNullOrWhiteSpace(name, nameof(name));
            Arguments = new Dictionary<string, NamedValueToken>();
        }
    }
}
