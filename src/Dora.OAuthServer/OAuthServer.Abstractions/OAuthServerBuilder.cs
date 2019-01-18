using Microsoft.Extensions.DependencyInjection;
using System;

namespace Dora.OAuthServer
{
    public class OAuthServerBuilder
    {
        public IServiceCollection Services { get; }
        public OAuthServerBuilder(IServiceCollection services)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));
        }
    }
    public class OAuthServerBuilder<TUser> : OAuthServerBuilder
    {
        public OAuthServerBuilder(IServiceCollection services) : base(services)
        {
        }
    }
}
