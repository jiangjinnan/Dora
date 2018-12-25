namespace Dora.OAuthServer
{
    /// <summary>
    /// Predefined Errors
    /// </summary>
    public static class OAuthErrors
	{
        /// <summary>
        /// invalid_request
        /// </summary>
        public static InvalidRequestCategory InvalidRequest { get; } = new InvalidRequestCategory();
        /// <summary>
        /// unauthorized_client
        /// </summary>
        public static UnauthorizedClientCategory UnauthorizedClient { get; } = new UnauthorizedClientCategory();
        /// <summary>
        /// access_denied
        /// </summary>
        public static AccessDeniedCategory AccessDenied { get; } = new AccessDeniedCategory();
        /// <summary>
        /// unsupported_response_type
        /// </summary>
        public static UnsupportedResponseTypeCategory UnsupportedResponseType { get; } = new UnsupportedResponseTypeCategory();
        /// <summary>
        /// unsupported_grant_type
        /// </summary>
        public static UnsupportedGrantTypeCategory UnsupportedGrantType { get; } = new UnsupportedGrantTypeCategory();
        /// <summary>
        /// invalid_scope
        /// </summary>
        public static InvalidScopeCategory InvalidScope { get; } = new InvalidScopeCategory();
        /// <summary>
        /// invalid_grant
        /// </summary>
        public static InvalidGrantCategory InvalidGrant { get; } = new InvalidGrantCategory();
        /// <summary>
        /// server_error
        /// </summary>
        public static ServerErrorCategory ServerError { get; } = new ServerErrorCategory();
    }
    /// <summary>
    /// invalid_request
    /// </summary>
	public class InvalidRequestCategory
	{
        /// <summary>
        /// Argument 'client_id' is not provided.
        /// </summary>
        public OAuthError MissingClientId { get; } = new OAuthError("E101", "invalid_request", "Argument 'client_id' is not provided.", 400);

        /// <summary>
        /// Argument 'client_secret' is not provided.
        /// </summary>
        public OAuthError MissingClientSecret { get; } = new OAuthError("E102", "invalid_request", "Argument 'client_secret' is not provided.", 400);

        /// <summary>
        /// Argument 'redirect_uri' is not provided.
        /// </summary>
        public OAuthError MissingRedirectUri { get; } = new OAuthError("E103", "invalid_request", "Argument 'redirect_uri' is not provided.", 400);

        /// <summary>
        /// Argument 'response_type' is not provided.
        /// </summary>
        public OAuthError MissingResponseType { get; } = new OAuthError("E104", "invalid_request", "Argument 'response_type' is not provided.", 400);

        /// <summary>
        /// Argument 'grant_type' is not provided.
        /// </summary>
        public OAuthError MissingGrantType { get; } = new OAuthError("E105", "invalid_request", "Argument 'grant_type' is not provided.", 400);

        /// <summary>
        /// Argument 'code' is not provided.
        /// </summary>
        public OAuthError MissingAuthorizationCode { get; } = new OAuthError("E106", "invalid_request", "Argument 'code' is not provided.", 400);

        /// <summary>
        /// Argument 'refresh_token' is not provided.
        /// </summary>
        public OAuthError MissingRefreshToken { get; } = new OAuthError("E107", "invalid_request", "Argument 'refresh_token' is not provided.", 400);

        /// <summary>
        /// Argument 'redirect_uri' is not a valid absolute URI.
        /// </summary>
        public OAuthError InvalidRedirectUri { get; } = new OAuthError("E108", "invalid_request", "Argument 'redirect_uri' is not a valid absolute URI.", 400);

        /// <summary>
        /// The method for the request to token endpoint must be POST
        /// </summary>
        public OAuthError UnsupportedHttpMethod { get; } = new OAuthError("E109", "invalid_request", "The method for the request to token endpoint must be POST", 400);

        /// <summary>
        /// The conent type for the request to token endpoint must be 'application/x-www-form-urlencoded'.
        /// </summary>
        public OAuthError UnsupportedContentType { get; } = new OAuthError("E110", "invalid_request", "The conent type for the request to token endpoint must be 'application/x-www-form-urlencoded'.", 400);

    }
    /// <summary>
    /// unauthorized_client
    /// </summary>
	public class UnauthorizedClientCategory
	{
        /// <summary>
        /// The provided 'client_id' ('{0}') is not registered.
        /// </summary>
        public OAuthError UnregisteredApplication { get; } = new OAuthError("E201", "unauthorized_client", "The provided 'client_id' ('{0}') is not registered.", 401);

        /// <summary>
        /// Access token is not provided.
        /// </summary>
        public OAuthError MissingAccessToken { get; } = new OAuthError("E202", "unauthorized_client", "Access token is not provided.", 401);

        /// <summary>
        /// Access token has expired.
        /// </summary>
        public OAuthError AccessTokenHasExpired { get; } = new OAuthError("E203", "unauthorized_client", "Access token has expired.", 401);

        /// <summary>
        /// Access token is revokded.
        /// </summary>
        public OAuthError AccessTokenIsRevoked { get; } = new OAuthError("E204", "unauthorized_client", "Access token is revokded.", 401);

        /// <summary>
        /// Access token's scopes do not match the resource endpoint based ones.
        /// </summary>
        public OAuthError UnauthorizedScopes { get; } = new OAuthError("E205", "unauthorized_client", "Access token's scopes do not match the resource endpoint based ones.", 403);

    }
    /// <summary>
    /// access_denied
    /// </summary>
	public class AccessDeniedCategory
	{
        /// <summary>
        /// The resource owner denied the access delegate.
        /// </summary>
        public OAuthError DelegateIsDenied { get; } = new OAuthError("E301", "access_denied", "The resource owner denied the access delegate.", 401);

        /// <summary>
        /// 输入的用户名不存在或者输入的密码不正确.
        /// </summary>
        public OAuthError InvalidCredential { get; } = new OAuthError("E302", "access_denied", "输入的用户名不存在或者输入的密码不正确.", 400);

        /// <summary>
        /// 请输入正确的用户名
        /// </summary>
        public OAuthError MissingUserName { get; } = new OAuthError("E303", "access_denied", "请输入正确的用户名", 400);

        /// <summary>
        /// 请输入正确的密码
        /// </summary>
        public OAuthError MissingPassword { get; } = new OAuthError("E304", "access_denied", "请输入正确的密码", 400);

    }
    /// <summary>
    /// unsupported_response_type
    /// </summary>
	public class UnsupportedResponseTypeCategory
	{
        /// <summary>
        /// The specified 'response_type' ('{0}') is not supported.
        /// </summary>
        public OAuthError UnsupportedResponseType { get; } = new OAuthError("E401", "unsupported_response_type", "The specified 'response_type' ('{0}') is not supported.", 400);

    }
    /// <summary>
    /// unsupported_grant_type
    /// </summary>
	public class UnsupportedGrantTypeCategory
	{
        /// <summary>
        /// The specified 'grant_type' ('{0}') is not supported.
        /// </summary>
        public OAuthError UnsupportedGrantType { get; } = new OAuthError("E501", "unsupported_grant_type", "The specified 'grant_type' ('{0}') is not supported.", 400);

    }
    /// <summary>
    /// invalid_scope
    /// </summary>
	public class InvalidScopeCategory
	{
        /// <summary>
        /// The provided scope '{0}' is not valid.
        /// </summary>
        public OAuthError InvalidScope { get; } = new OAuthError("E601", "invalid_scope", "The provided scope '{0}' is not valid.", 400);

    }
    /// <summary>
    /// invalid_grant
    /// </summary>
	public class InvalidGrantCategory
	{
        /// <summary>
        /// The provided authorization code is malformed.
        /// </summary>
        public OAuthError InvalidAuthorizationCode { get; } = new OAuthError("E701", "invalid_grant", "The provided authorization code is malformed.", 400);

        /// <summary>
        /// The provided authorization code has expired.
        /// </summary>
        public OAuthError AuthorizationCodeHasExpired { get; } = new OAuthError("E702", "invalid_grant", "The provided authorization code has expired.", 400);

        /// <summary>
        /// The provided authorization code is revoked.
        /// </summary>
        public OAuthError AuthorizationCodeIsRevoked { get; } = new OAuthError("E703", "invalid_grant", "The provided authorization code is revoked.", 400);

        /// <summary>
        /// The provided refresh token is malformed.
        /// </summary>
        public OAuthError InvalidRefreshToken { get; } = new OAuthError("E704", "invalid_grant", "The provided refresh token is malformed.", 400);

        /// <summary>
        /// The provided refresh token has expired.
        /// </summary>
        public OAuthError RefreshTokenHasExpired { get; } = new OAuthError("E705", "invalid_grant", "The provided refresh token has expired.", 400);

        /// <summary>
        /// The provided refresh token is revoked.
        /// </summary>
        public OAuthError RefreshTokenIsRevoked { get; } = new OAuthError("E706", "invalid_grant", "The provided refresh token is revoked.", 400);

        /// <summary>
        /// The provided access token is malformed.
        /// </summary>
        public OAuthError InvalidAccessToken { get; } = new OAuthError("E707", "invalid_grant", "The provided access token is malformed.", 400);

        /// <summary>
        /// The provided access token has expired.
        /// </summary>
        public OAuthError AccessTokenHasExpired { get; } = new OAuthError("E708", "invalid_grant", "The provided access token has expired.", 400);

        /// <summary>
        /// The provided access token is revoked.
        /// </summary>
        public OAuthError AccessTokenIsRevoked { get; } = new OAuthError("E709", "invalid_grant", "The provided access token is revoked.", 400);

        /// <summary>
        /// The provided 'redirect_uri' does not match any of registered ones.
        /// </summary>
        public OAuthError InvalidRedirectUri { get; } = new OAuthError("E710", "invalid_grant", "The provided 'redirect_uri' does not match any of registered ones.", 400);

        /// <summary>
        /// The provided 'client_secret' does not match the registered one.
        /// </summary>
        public OAuthError InvalidClientSecret { get; } = new OAuthError("E711", "invalid_grant", "The provided 'client_secret' does not match the registered one.", 400);

    }
    /// <summary>
    /// server_error
    /// </summary>
	public class ServerErrorCategory
	{
        /// <summary>
        /// Error happens during request is processed on server.
        /// </summary>
        public OAuthError ServerError { get; } = new OAuthError("E801", "server_error", "Error happens during request is processed on server.", 500);

    }
}