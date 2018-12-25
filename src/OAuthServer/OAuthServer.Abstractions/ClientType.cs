namespace Dora.OAuthServer
{
    /// <summary>
    /// Represent the OAuth 2.0 client type (refer to <see cref="!:https://tools.ietf.org/html/rfc6749#section-2.1"/>)
    /// </summary>
    public enum ClientType
    {
        /// <summary>
        /// Clients capable of maintaining the confidentiality of their credentials or capable of secure client authentication using other means.
        /// </summary>
        /// <example>
        /// The client implemented on a secure server with restricted access to the client credentials.
        /// </example>
        Confidential = 0,

        /// <summary>
        /// Clients incapable of maintaining the confidentiality of their credentials, and incapable of secure client authentication via any other means.
        /// </summary>
        /// <example>
        /// Clients executing on the device used by the resource owner, such as an installed native application or a web browser-based application.
        /// </example>
        Public = 1
    }
}