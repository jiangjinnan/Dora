using System.Threading.Tasks;

namespace Dora.OAuthServer
{
    /// <summary>
    /// Represents the store in which the current authorization code, refresh token and access token is stored.
    /// </summary>
    public interface IOAuthGrantStore
    {
        /// <summary>
        /// Set a new authorization code.
        /// </summary>
        /// <param name="clientId">Client identifier of the application to register.</param>
        /// <param name="userName">The user name of the resoruce owner.</param>
        /// <param name="authorizationCode">The new authorization code to set.</param>
        /// <returns>The task to set the new authorization code.</returns>
        /// <remarks>Once the authorization code is reset, the original authorization code and specific issued refresh token and access token will be revoked.</remarks>
        Task UpdateAuthorizationCodeAsync(string clientId, string userName, string authorizationCode);

        /// <summary>
        /// Set a new refresh token and access token.
        /// </summary>
        /// <param name="clientId">Client identifier of the application to register.</param>
        /// <param name="userName">The user name of the resoruce owner.</param>
        /// <param name="refreshToken">The new refresh token to set.</param>
        /// <param name="accessToken">The new access token to set.</param>
        /// <returns>The task to set the new refresh token.</returns>
        /// <remarks>Once the tokens is reset, the original authorization code , refresh token and access token will be revoked.</remarks>
        Task UpdateTokensAsync(string clientId, string userName, string refreshToken, string accessToken);

        /// <summary>
        /// Check if the specified authorization code is valid.
        /// </summary>
        /// <param name="authorizationCode">The authorization code to check.</param>
        /// <returns>The task to validate the specified authorization code.</returns>
        Task<bool> VaidateAuthorizationCodeAsync(string authorizationCode);

        /// <summary>
        /// Check if the specified refresh token is valid.
        /// </summary>
        /// <param name="refreshToken">The refresh token to check.</param>
        /// <returns>The task to validate the specified refresh token.</returns>
        Task<bool> VaidateRefreshTokenAsync(string refreshToken);

        /// <summary>
        /// Check if the specified access token is valid.
        /// </summary>
        /// <param name="accessToken">The access token to check.</param>
        /// <returns>The task to validate the specified access token.</returns>
        Task<bool> VaidateAccessTokenAsync(string accessToken);

        /// <summary>
        /// Get user name of the resource owner.
        /// </summary>
        /// <param name="authorizationCode">The authorization code issued to the resource owner.</param>
        /// <returns>The task to get user name of rource owner.</returns>
        Task<string> GetUserNameAsync(string authorizationCode);

        /// <summary>
        /// Revoke the specfied issued access token.
        /// </summary>
        /// <param name="accessToken">The access token to revoke.</param>
        /// <returns>The task to revoke the specified access token.</returns>
        Task RevokeAccessTokenAsync(string accessToken);
    }
}
