using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.OAuthServer
{
    internal static class HttpContextExtensions
    {
        public static string GetRequestUrl(this HttpContext httpContext)
        {
            var request = httpContext.Request;
            return $"{request.Scheme}://{request.Host}{request.PathBase}{request.Path}";
            //$"{request.QueryString}";
        }
    }
}
