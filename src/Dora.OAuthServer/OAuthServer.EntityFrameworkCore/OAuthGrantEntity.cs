namespace Dora.OAuthServer
{
    /// <summary>
    /// The <see cref="OAuthGrant"/> specific EF entity class.
    /// </summary>
    public class OAuthGrantEntity
    {
        #region Properties
        /// <summary>
        /// Client identifier of the application to register.
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// User name of the resource owner.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// The current authorization code.
        /// </summary>
        public string AuthorizationCode { get; set; }

        /// <summary>
        /// The current refresh token.
        /// </summary>
        public string RefreshToken { get; set; }

        /// <summary>
        /// The current access token.
        /// </summary>
        public string AccessToken { get; set; }
        #endregion

        /// <summary>
        /// Creates a new <see cref="OAuthGrantEntity"/>.
        /// </summary>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="authorizationCode">The authorization code.</param>
        /// <param name="accessToken">The access token.</param>
        /// <param name="refreshToken">The refresh token.</param>
        /// <param name="normalizer">The normalizer.</param>
        /// <returns></returns>
        public static OAuthGrantEntity Create(
            string clientId, 
            string userName,
            string authorizationCode,
            string accessToken,
            string refreshToken,
            ILookupNormalizer normalizer)
        {
            Guard.ArgumentNotNullOrWhiteSpace(clientId, nameof(clientId));
            Guard.ArgumentNotNullOrWhiteSpace(userName, nameof(userName));
            Guard.ArgumentNotNull(normalizer, nameof(normalizer));

            return new OAuthGrantEntity
            {
                ClientId = normalizer.Normalize(clientId),
                UserName = normalizer.Normalize(userName),
                AuthorizationCode = authorizationCode,
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }
    }
}