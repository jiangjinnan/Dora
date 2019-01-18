namespace Dora.OAuthServer
{
    /// <summary>
    /// Represents OAuth ressponse type.
    /// </summary>
    public enum OAuthGrantType
    {
        /// <summary>
        /// Authorization Code
        /// </summary>
        AuthorizationCode,
        //Password,
        //ClientCredential,

        /// <summary>
        /// Refresh Token
        /// </summary>
        RefreshToken
    }
}