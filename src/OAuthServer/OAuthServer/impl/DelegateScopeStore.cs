using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dora.OAuthServer
{
    /// <summary>
    /// The configuration based resoruce access delegate scope store.
    /// </summary>
    public class DelegateScopeStore : IDelegateScopeStore
    {
        private DelegateScope[] _scopes;

        /// <summary>
        /// Creates a new <see cref="DelegateScopeStore"/>.
        /// </summary>
        /// <param name="optionsAccessor">A <see cref="IOptions{AuthorizationServerOptions}"/> to get <see cref="AuthorizationServerMiddleware"/> based configuration options.</param>
        /// <exception cref="ArgumentNullException">Specified <paramref name="optionsAccessor"/> is null.</exception>
        public DelegateScopeStore(IOptions<OAuthServerOptions> optionsAccessor)
        {
            Guard.ArgumentNotNull(optionsAccessor, nameof(optionsAccessor));
            _scopes = optionsAccessor.Value.AuthorizationServer.Scopes.Select(it => it.ToOAuthScope()).ToArray();
        }

        /// <summary>
        /// Get all resource access delegate scopes.
        /// </summary>
        /// <returns>the task to get all resource access delegate scopes. </returns>
        public virtual Task<IEnumerable<DelegateScope>> GetAllAsync()
        {
            return Task.FromResult<IEnumerable<DelegateScope>>(_scopes);
        }
    }
}
