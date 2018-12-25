using System.Collections.Generic;

namespace Dora.OAuthServer
{
    /// <summary>
    /// Represent the resource owner's consent to client applicaiton's delegate.
    /// </summary>
    public class DelegateConsent
    {
        #region Properties
        /// <summary>
        /// The client identifier of the application to delegate the resource owner.
        /// </summary>
        public string ClientId { get; }

        /// <summary>
        /// The user name of the resource owner delegated by client application.
        /// </summary>
        public string UserName { get; }

        /// <summary>
        /// The OAuth scopes to delegate.
        /// </summary>
        public IEnumerable<string> Scopes{ get; }

        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new <see cref="DelegateConsent"/>.
        /// </summary>
        /// <param name="clientId">The client identifier of the application to delegate the resource owner.</param>
        /// <param name="userName">The user name of the resource owner delegated by client application.</param>
        /// <param name="scopes">The OAuth scopes to delegate.</param>
        public DelegateConsent(string clientId, string userName, IEnumerable<string> scopes)
        {
            ClientId = Guard.ArgumentNotNullOrWhiteSpace(clientId, nameof(clientId));
            UserName = Guard.ArgumentNotNullOrWhiteSpace(userName, nameof(userName));
            Scopes = Guard.ElementNotNullOrWhiteSpace(scopes, nameof(scopes), false);
        }
        #endregion
    }
}
