using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Dora.GraphQL.GraphTypes;

namespace Dora.GraphQL.Selections.impl
{
    public class Fragment : IFragment
    {       
        public IGraphType GraphType { get; }
        public ICollection< ISelectionNode> SelectionSet { get; }
        public Fragment(IGraphType graphType)
        {
            GraphType = Guard.ArgumentNotNull( graphType, nameof(graphType));
            SelectionSet = new Collection<ISelectionNode>();
        }
    }
}
