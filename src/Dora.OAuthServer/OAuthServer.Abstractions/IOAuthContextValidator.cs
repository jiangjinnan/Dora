using System.Threading.Tasks;

namespace Dora.OAuthServer
{
    /// <summary>
    /// The validator to check if the OAuth request context is valid.
    /// </summary>
    public interface IOAuthContextValidator
    {
        /// <summary>
        /// Validate the authorization endpoint specific context.
        /// </summary>
        /// <param name="context">The authorization endpoint specific context.</param>
        /// <returns>The task to validate the authorization endpoint specific context.</returns>
        Task ValidateAuthorizationContextAsync(AuthorizationContext context);

        /// <summary>
        /// Validate the token endpoint specific context.
        /// </summary>
        /// <param name="context">The authorization endpoint specific context.</param>
        /// <returns>The task to validate the token endpoint specific context.</returns>
        Task ValidateTokenGrantContextAsync(TokenContext context);

        /// <summary>
        /// Validates resource accessing context.
        /// </summary>
        /// <param name="context">The resource accessing context.</param>
        /// <returns>The task to validate the resource accessing context.</returns>
        Task ValidateResourceContextAsync(ResourceContext context);
    }
}
