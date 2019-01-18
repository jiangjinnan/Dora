using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dora.OAuthServer
{
    /// <summary>
    /// The store in which the OAuth scopes are stored.
    /// </summary>
    public interface IDelegateScopeStore
    {
        /// <summary>
        /// Get all resource access delegate scopes.
        /// </summary>
        /// <returns>the task to get all resource access delegate scopes. </returns>
        Task<IEnumerable<DelegateScope>> GetAllAsync();
    }
}