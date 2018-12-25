using System;

namespace Dora.OAuthServer
{
    /// <summary>
    /// Defines some extension methods for <see cref="AccessTokenValidationContext"/>.
    /// </summary>
    public static class AccessTokenValidationContextExtensions
    {
        /// <summary>
        /// Validation fails and set the specified error.
        /// </summary>
        /// <param name="context">The <see cref="AccessTokenValidationContext"/> representing the execution context in which the access token validation is performed.</param>
        /// <param name="error">The <see cref="OAuthError"/> to set.</param>
        /// <param name="arguments">The arguments used to replace the placeholders of description template.</param>
        /// <returns>The <see cref="OAuthRequestContext"/> with specified error.</returns>
        /// <exception cref="ArgumentNullException">Specified <paramref name="context"/> is null.</exception>
        /// <exception cref="ArgumentNullException">Specified <paramref name="error"/> is null.</exception>
        public static AccessTokenValidationContext Failed(this AccessTokenValidationContext context, OAuthError error, params object[] arguments)
        {
            Guard.ArgumentNotNull(context, nameof(context));
            Guard.ArgumentNotNull(error, nameof(error));

            context.Error = error.Format(arguments);
            return context;
        }
    }
}
