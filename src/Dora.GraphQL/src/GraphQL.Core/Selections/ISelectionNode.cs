using System.Collections.Generic;

namespace Dora.GraphQL.Selections
{
    public interface ISelectionNode
    {
        ICollection<ISelectionNode> SelectionSet { get; }
    }
}