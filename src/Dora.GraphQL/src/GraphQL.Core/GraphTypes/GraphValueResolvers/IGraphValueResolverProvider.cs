using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.GraphQL.GraphTypes
{
    public interface IGraphValueResolverProvider
    {
        GraphValueResolver GetGraphTypeResolver(Type type);
    }

    
}
