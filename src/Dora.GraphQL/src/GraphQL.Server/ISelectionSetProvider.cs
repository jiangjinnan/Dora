using Dora.GraphQL.Selections;
using GraphQL.Language.AST;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.GraphQL.Server
{
    public interface ISelectionSetProvider
    {
        ICollection<ISelectionNode> GetSelectionSet(string query, Operation operation, Fragments fragments);
    }
}
