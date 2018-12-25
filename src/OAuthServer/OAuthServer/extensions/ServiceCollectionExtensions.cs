using Dora.OAuthServer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddOAuthServer<TUser>(this IServiceCollection services, Action<OAuthServerBuilder<TUser>> configure)
        {
            Guard.ArgumentNotNull(services, nameof(services));
            Guard.ArgumentNotNull(services, nameof(configure));

            services.TryAddSingleton<ISecureDataFormat<OAuthTicket>>(provider =>
            {
                var protector = provider.GetRequiredService<IDataProtectionProvider>().CreateProtector(OAuthDefaults.ProtectionPurpose);
                return new OAuthTicketFormat(protector);
            });

            services.TryAddSingleton<IDataSerializer<OAuthTicket>>(OAuthTicketSerializer.Instance);
            services.TryAddSingleton<IAccessTokenAccessor, AccessTokenAccessor>();
            services.TryAddSingleton<IDelegateScopeStore, DelegateScopeStore>();
            services.TryAddSingleton<ILookupNormalizer, LookupNormalizer>();
            services.TryAddSingleton<IOAuthContextFactory, OAuthContextFactory>();
            services.TryAddSingleton<ITokenValueGenerator, TokenValueGenerator>();
            services.TryAddScoped<IOAuthContextValidator, OAuthContextValidator>();
            configure(new OAuthServerBuilder<TUser>(services));
            return services;
        }
    }
}
