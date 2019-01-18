using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Dora.OAuthServer
{
    /// <summary>
    /// Represents the store of OAuth delegatioon consent.
    /// </summary>
    public interface IDelegateConsentStore
    {
        /// <summary>
        /// Add new OAuth delegate consent.
        /// </summary>
        /// <param name="clientId">Client identifier of the application.</param>
        /// <param name="userName">User name of resource owner who delegate the client application to get access his/her resource.</param>
        /// <param name="scopes">The delegate scopes.</param>
        /// <returns>The task to add OAuth delegate scope.</returns>
        Task AddAsync(string clientId, string userName, IEnumerable<string> scopes);

        /// <summary>
        /// Revoke all delegate scopes assigned to specified user.
        /// </summary>
        /// <param name="clientId">Client identifier of the application.</param>
        /// <param name="userName">User name of resource owner who delegate the client application to get access his/her resource.</param>
        /// <returns>The task to revoke all delegate scopes assigned to specified user. </returns>
        Task RemoveAllScopesAsync(string clientId, string userName);

        /// <summary>
        /// Revoke specified delegate scopes assigned to a user.
        /// </summary>
        /// <param name="clientId">Client identifier of the application.</param>
        /// <param name="userName">User name of resource owner who delegate the client application to get access his/her resource.</param>
        /// <param name="scopes">The delegate scopes to remove.</param>
        /// <returns>The task revoke specified delegate scopes assigned to a user. </returns>
        Task RemoveScopesAsync(string clientId, string userName, IEnumerable<string> scopes);

        /// <summary>
        /// Get all delegate scopes assigned to specified user.
        /// </summary>
        /// <param name="clientId">Client identifier of the application.</param>
        /// <param name="userName">User name of resource owner who delegate the client application to get access his/her resource.</param>
        /// <returns>The task to get all delegate scopes assigned to specified user. </returns>
        Task<DelegateConsent> GetAsync(string clientId, string userName );

        /// <summary>
        /// Get all delegate scopes assigned to specified user.
        /// </summary>
        /// <param name="userName">User name of resource owner who delegate the client application to get access his/her resource.</param>
        /// <returns>The task to get all delegate scopes assigned to specified user. </returns>
        Task<IEnumerable<DelegateConsent>> GetAsync(string userName);
    }
}
