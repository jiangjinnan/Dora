using Dora.GraphQL.Executors;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.GraphQL.Server
{
    public interface IGraphContextFeature
    {
        GraphContext GraphContext { get; }
    }
}
