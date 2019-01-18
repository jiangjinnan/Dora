using System;

namespace Dora.OAuthServer
{
    /// <summary>
    /// Defines some extension methods for <see cref="OAuthContext"/>.
    /// </summary>
    public static class OAuthRequestContextExtensions
    {
        /// <summary>
        /// Validation fails and set the specified error.
        /// </summary>
        /// <param name="context">The <see cref="OAuthContext"/> representing the request context to authorization server endpoints.</param>
        /// <param name="error">The <see cref="OAuthError"/> to set.</param>
        /// <param name="arguments">The arguments used to replace the placeholders of description template.</param>
        /// <returns>The <see cref="OAuthContext"/> with specified error.</returns>
        /// <exception cref="ArgumentNullException">Specified <paramref name="context"/> is null.</exception>
        /// <exception cref="ArgumentNullException">Specified <paramref name="error"/> is null.</exception>
        public static OAuthContext Failed(this OAuthContext context, OAuthError error, params object[] arguments)
        {
            Guard.ArgumentNotNull(context, nameof(context));
            Guard.ArgumentNotNull(error, nameof(error));

            context.Error = error.Format(arguments);
            return context;
        }
    }
}
