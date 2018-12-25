namespace Dora.OAuthServer
{
    /// <summary>
    /// The response type for authorization endpoint.
    /// </summary>
    public enum OAuthResponseType
    {
        /// <summary>
        /// Return the authorization code. (Authorization Code Grant)
        /// </summary>
        AuthorizationCode,

        /// <summary>
        /// return the access token. (Implicit Grant)
        /// </summary>
        AccessToken
    }
}
