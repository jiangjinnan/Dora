using Dora.GraphQL.Executors;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Dora.GraphQL.Server
{
    public interface IGraphContextFactory
    {
        ValueTask<GraphContext> CreateAsync(RequestPayload payload);
    }
}
