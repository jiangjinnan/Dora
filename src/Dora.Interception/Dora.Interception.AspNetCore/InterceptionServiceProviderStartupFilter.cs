using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Builder;

namespace Dora.Interception.AspNetCore
{
    public class InterceptionServiceProviderStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return builder =>
            {
                builder.UseMiddleware<InterceptionServiceProviderMiddleware>();
                next(builder);
            };
        }
    }
}
