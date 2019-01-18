namespace Dora.OAuthServer
{
    /// <summary>
    /// A generator to generate the authorization code, refresh token and access token.
    /// </summary>
    public interface ITokenValueGenerator
    {
        /// <summary>
        /// Generate authorization code.
        /// </summary>
        /// <returns>The generated authorization code.</returns>
        string GenerateAuthorizationCode();

        /// <summary>
        /// Generate refresh token.
        /// </summary>
        /// <returns>The generated refresh token.</returns>
        string GenerateRefreshToken();

        /// <summary>
        /// Generate access token.
        /// </summary>
        /// <returns>The generated access token..</returns>
        string GenerateAccessToken();
    }
}
