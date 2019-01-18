using System.Linq;
using System;


namespace Dora.OAuthServer
{
    /// <summary>
    /// The <see cref="ResourceAccessDelegateConsent"/> specific EF entity class.
    /// </summary>
    public class DelegateConsentEntity
    {
        #region Properties
        /// <summary>
        /// Client identifier of the registered application.
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// User name of the resource owner whose delegate the client application to access his/her resource.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// The delegate scopes.
        /// </summary>
        public string Scopes { get; set; }
        #endregion
       

        #region Public Methods

        public static DelegateConsentEntity Create(DelegateConsent consent, ILookupNormalizer normalizer)
        {
            Guard.ArgumentNotNull(consent, nameof(consent));
            Guard.ArgumentNotNull(normalizer, nameof(normalizer));
            return new DelegateConsentEntity
            {
                ClientId = normalizer.Normalize(consent.ClientId),
                UserName = normalizer.Normalize(consent.UserName),
                Scopes = string.Join(Constants.SeperatorString.ToString(), consent.Scopes.Select(it => normalizer.Normalize(it)))
            };
        }

        /// <summary>
        /// Convert current <see cref="DelegateConsentEntity"/> to an <see cref="ResourceAccessDelegateConsent"/>.
        /// </summary>
        /// <returns>The <see cref="ResourceAccessDelegateConsent"/> to which the current <see cref="DelegateConsentEntity"/> is converted.</returns>
        public DelegateConsent ToOAuthDelegateConsent()
        {
            return new DelegateConsent(ClientId, UserName, Scopes.Split(Constants.SeperatorCharacter));
        }
        #endregion
    }
}
