using System;

namespace Dora.OAuthServer
{
    /// <summary>
    /// Defines the default options about authorization server.
    /// </summary>
    public static class AuthorizationServerOptionDefaults
    {
        /// <summary>
        /// The URL of authorization endpoint.
        /// </summary>
        public static string AuthorizationEndpoint { get; } = $"/authorize";

        /// <summary>
        ///  The URL of token endpoint.
        /// </summary>
        public static string TokenEndpoint = $"/token";

        /// <summary>
        ///  The URL of token endpoint.
        /// </summary>
        public static string SignOutEndpoint = $"/signout";


        /// <summary>
        ///  The URL of login page.
        /// </summary>
        public static string LoginPageUrl = $"/account/logon";

        /// <summary>
        /// The URL of page the collect user's delegate decision.
        /// </summary>
        public static string DelegateConsentPageUrl = $"/account/delegateconsent";

        /// <summary>
        /// Default expiration duration for authorization code.
        /// </summary>
        public static TimeSpan AuthorizationCodeExpiration { get; } = TimeSpan.FromMinutes(1);

        /// <summary>
        /// Default expiration duration for access token.
        /// </summary>
        public static TimeSpan AccessTokenExpiration { get; } = TimeSpan.FromHours(1);

        /// <summary>
        /// Default expiration duration for refresh token.
        /// </summary>
        public static TimeSpan RefreshTokenExpiration { get; } = TimeSpan.FromHours(10);

        /// <summary>
        /// The name of access token type.
        /// </summary>
        public const string TokenType = "Bearer";
    }
}
