using Dora.GraphQL.GraphTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.GraphQL.Selections
{
    public interface IFragment
    {
         string Name { get; }
         IGraphType GraphType { get; }
         ICollection<ISelectionNode> SelectionSet { get; }
    }
}
