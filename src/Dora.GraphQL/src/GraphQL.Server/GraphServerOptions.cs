using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.GraphQL.Server
{
    public class GraphServerOptions: GraphOptions
    {
        public PathString PathBase { get; set; } = "/graphql";
    }
}
