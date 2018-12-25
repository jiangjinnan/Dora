using System;
using System.Globalization;

namespace Dora.OAuthServer
{
    /// <summary>
    /// Represents an OAuth based error.
    /// </summary>
    public class OAuthError
    {
        #region Properties
        /// <summary>
        /// The error code.
        /// </summary>
        public string Code { get; }

        /// <summary>
        /// The error category.
        /// </summary>
        public string Category { get; }

        /// <summary>
        /// The error description template with some placeholders.
        /// </summary>
        public string Template { get; }

        /// <summary>
        /// The HTTP response status code.
        /// </summary>
        public int StatusCode { get; }
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new <see cref="OAuthError"/>.
        /// </summary>
        /// <param name="code">The error code.</param>
        /// <param name="category">The error category.</param>
        /// <param name="template">The error description template with some placeholders.</param>
        /// <param name="statusCode">The HTTP response status code.</param>
        /// <exception cref="ArgumentNullException">Specified <paramref name="code"/> is null.</exception>
        /// <exception cref="ArgumentNullException">Specified <paramref name="category"/> is null.</exception>
        /// <exception cref="ArgumentNullException">Specified <paramref name="template"/> is null.</exception>
        /// <exception cref="ArgumentException">Specified <paramref name="code"/> is a white space string.</exception>
        /// <exception cref="ArgumentException">Specified <paramref name="category"/> is a white space string.</exception>
        /// <exception cref="ArgumentException">Specified <paramref name="template"/> is a white space string.</exception>
        public OAuthError(string code, string category, string template, int statusCode)
        {
            this.Code = Guard.ArgumentNotNullOrWhiteSpace(code, nameof(code));
            this.Category = Guard.ArgumentNotNullOrWhiteSpace(category, nameof(category));
            this.Template = Guard.ArgumentNotNullOrWhiteSpace(template, nameof(template));
            this.StatusCode = statusCode;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Format the error description and generate a <see cref="ResponseError"/>.
        /// </summary>
        /// <param name="arguments">The arguments to replace the placeholders defined in descripton template.</param>
        /// <returns>The generated <see cref="ResponseError"/> with a complete error description.</returns>
        public ResponseError Format(params object[] arguments)
        {
            string description = string.Format(CultureInfo.CurrentCulture,this.Template, arguments);
            return new ResponseError(this.Category, $"[{this.Code}]{description}", this.StatusCode);
        }
        #endregion
    }
}