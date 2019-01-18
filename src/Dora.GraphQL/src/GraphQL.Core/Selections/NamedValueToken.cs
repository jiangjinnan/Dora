using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.GraphQL.Selections
{
    public struct NamedValueToken
    {    
        public string Name { get; }
        public string ValueToken { get; }
        public NamedValueToken(string name, string valueToken) : this()
        {
            Name = Guard.ArgumentNotNullOrWhiteSpace( name, nameof(name));
            ValueToken = Guard.ArgumentNotNullOrWhiteSpace(valueToken, nameof(valueToken)); 
        }
    }
}
