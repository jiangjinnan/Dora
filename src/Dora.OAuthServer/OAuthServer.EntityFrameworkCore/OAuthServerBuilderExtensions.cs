using Dora.OAuthServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class OAuthServerBuilderExtensions
    {
        /// <summary>
        /// Registers the <see cref="OAuthDbStore{TDbContext}"/> to specified <see cref="AuthorizationServerBuilder"/>.
        /// </summary>
        /// <typeparam name="TDbContext">The type of <see cref="DbContext"/>.</typeparam>
        /// <param name="builder">The <see cref="AuthorizationServerBuilder"/> to which the <see cref="OAuthDbStore{TDbContext}"/> is registered.</param>
        /// <returns>The <see cref="AuthorizationServerBuilder"/> with the new registration.</returns>
        /// <exception cref="ArgumentNullException">Specified <paramref name="builder"/> is null.</exception>
        public static OAuthServerBuilder AddEntityFrameworkStore<TDbContext>(this OAuthServerBuilder builder)
            where TDbContext : DbContext
        {
            Guard.ArgumentNotNull(builder, nameof(builder));

            builder.Services.TryAddScoped<IApplicationStore, OAuthDbStore<TDbContext>>();
            builder.Services.TryAddScoped<IDelegateConsentStore, OAuthDbStore<TDbContext>>();
            builder.Services.TryAddScoped<IOAuthGrantStore, OAuthDbStore<TDbContext>>();

            return builder;
        }

        public static OAuthServerBuilder<TUser> AddEntityFrameworkStore<TUser,TDbContext>(this OAuthServerBuilder<TUser> builder)
            where TDbContext : DbContext
        {
            AddEntityFrameworkStore<TDbContext>((OAuthServerBuilder)builder);
            return builder;
        }
    }
}
