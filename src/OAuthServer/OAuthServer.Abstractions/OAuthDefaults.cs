using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.OAuthServer
{
    public static class OAuthDefaults
    {
        public const string ProtectionPurpose = "AUTHORIZARION_CODE_TOKEN_ENCRYPTION";

        /// <summary>
        /// Common paramter names.
        /// </summary>
        public static class ParameterNames
        {
            /// <summary>
            /// Client identifier of registered application.
            /// </summary>
            public const string ClientId = "client_id";

            /// <summary>
            /// Client secret of registered application.
            /// </summary>
            public const string ClientSecret = "client_secret";

            /// <summary>
            /// Response type for authorization endpoint.
            /// </summary>
            public const string ResponseType = "response_type";

            /// <summary>
            /// Redirect URI of registered application.
            /// </summary>
            public const string RedirectUri = "redirect_uri";

            /// <summary>
            /// The scopes bound to the issued authorization code, refresh token or access token.
            /// </summary>
            public const string Scope = "scope";

            /// <summary>
            /// The state passed from client to prevent cross-site request forgery.
            /// </summary>
            public const string State = "state";

            /// <summary>
            /// The provided authorization code to token endpoint.
            /// </summary>
            public const string Code = "code";

            /// <summary>
            /// The OAuth grant type.
            /// </summary>
            public const string GarntType = "grant_type";

            /// <summary>
            /// The provided refresh token to token endpoint.
            /// </summary>
            public const string RefreshToken = "refresh_token";
        }
    }
}
