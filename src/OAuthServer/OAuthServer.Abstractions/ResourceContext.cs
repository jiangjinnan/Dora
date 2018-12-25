using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dora.OAuthServer
{
    public class ResourceContext: OAuthContext
    {              
        public OAuthTicket OAuthTicket { get; }
        public ResourceEndpoint ResourceEndpoint { get; }
        public IEnumerable<string> Scopes { get; }
        
        internal ResourceContext(HttpContext httpContext, ResponseError error) : base(httpContext, error)
        { }

        public ResourceContext(HttpContext httpContext, OAuthTicket oAuthTicket, ResourceEndpoint resourceEndpoint):base(httpContext)
        {
            OAuthTicket = oAuthTicket ?? throw new ArgumentNullException(nameof(oAuthTicket));
            ResourceEndpoint = resourceEndpoint ?? throw new ArgumentNullException(nameof(resourceEndpoint));
            Scopes = oAuthTicket.Scopes.Intersect(resourceEndpoint.Scopes).ToArray();
        }
    }
}
