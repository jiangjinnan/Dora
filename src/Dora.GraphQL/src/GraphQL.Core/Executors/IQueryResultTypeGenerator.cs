using Dora.GraphQL.GraphTypes;
using Dora.GraphQL.Selections;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.GraphQL.Executors
{
    public interface IQueryResultTypeGenerator
    {
        Type Generate(IFieldSelection selection, GraphField field);

    }
}
