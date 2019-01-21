using Dora.GraphQL.GraphTypes;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Dora.GraphQL.Selections
{
    public class Fragment : IFragment
    {       
        public IGraphType GraphType { get; }
        public ICollection< ISelectionNode> SelectionSet { get; }
        public IDictionary<string, object> Properties { get; }
        public Fragment(IGraphType graphType)
        {
            GraphType = Guard.ArgumentNotNull( graphType, nameof(graphType));
            SelectionSet = new Collection<ISelectionNode>();
            Properties = new Dictionary<string, object>();
        }
    }
}
