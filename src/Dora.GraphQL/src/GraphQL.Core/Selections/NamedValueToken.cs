using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.GraphQL.Selections
{
    public struct NamedValueToken
    {    
        public string Name { get; }
        public object ValueToken { get; }
        public NamedValueToken(string name, object valueToken) : this()
        {
            Name = Guard.ArgumentNotNullOrWhiteSpace( name, nameof(name));
            ValueToken = Guard.ArgumentNotNull(valueToken, nameof(valueToken)); 
        }
    }
}
