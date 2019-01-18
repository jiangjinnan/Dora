using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dora.OAuthServer
{
    /// <summary>
    /// Represent registered application store.
    /// </summary>
    public interface IApplicationStore
    {
        /// <summary>
        /// Create and store a new application registration.
        /// </summary>
        /// <param name="application">The application to create.</param>
        Task CreateAsync(Application application);

        /// <summary>
        /// Update the information of an existing registered application.
        /// </summary>
        /// <param name="application">The existing registered application to update.</param>
        /// <returns>The task to update the existing registered application's informaton.</returns>
        Task UpdateAsync(Application application);

        /// <summary>
        /// Get the registered application based on client Id.
        /// </summary>
        /// <param name="clientId">Client identifer of the application to get.</param>
        /// <returns>The task to get the specified application.</returns>
        Task<Application> GetByClientIdAsync(string clientId);

        /// <summary>
        /// Get the registered applications owned by specified user.
        /// </summary>
        /// <param name="owner">The application's owner.</param>
        /// <returns>The task to get the applications.</returns>
        Task<IEnumerable<Application>> GetOneByUserNameAsync(string owner);

        /// <summary>
        /// Delete the specified application.
        /// </summary>
        /// <param name="clientId">The client Id of the application to delete.</param>
        /// <returns>The task to delete the specified application.</returns>
        Task DeleteAsync(string clientId);
    }
}
