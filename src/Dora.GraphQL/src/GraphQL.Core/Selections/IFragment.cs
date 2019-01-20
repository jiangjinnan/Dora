using Dora.GraphQL.GraphTypes;
using System.Collections.Generic;

namespace Dora.GraphQL.Selections
{
    public interface IFragment: ISelectionNode
    {
         IGraphType GraphType { get; }
    }
}
