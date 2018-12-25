using System;

namespace Dora.OAuthServer
{
    /// <summary>
    /// Represents the standard error described in OAuth specification.
    /// </summary>
    public class ResponseError
    {
        #region Properties
        /// <summary>
        /// The error category.
        /// </summary>
        public string Error { get; }

        /// <summary>
        /// The errror description.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// The HTTP response status code.
        /// </summary>
        public int StatusCode { get; }
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new <see cref="ResponseError"/>.
        /// </summary>
        /// <param name="error">The error category.</param>
        /// <param name="description">The errror description.</param>
        /// <param name="statusCode">The HTTP response status code.</param>
        /// <exception cref="ArgumentNullException">Specified <paramref name="error"/> is null.</exception>
        /// <exception cref="ArgumentNullException">Specified <paramref name="description"/> is null.</exception>
        /// <exception cref="ArgumentException">Specified <paramref name="error"/> is a white space string.</exception>
        /// <exception cref="ArgumentException">Specified <paramref name="description"/> is a white space string.</exception>
        public ResponseError(string error, string description, int statusCode)
        {
            Error = Guard.ArgumentNotNullOrWhiteSpace(error, nameof(error));
            Description = Guard.ArgumentNotNullOrWhiteSpace(description, nameof(description));
            StatusCode = statusCode;
        }
        #endregion
    }
}