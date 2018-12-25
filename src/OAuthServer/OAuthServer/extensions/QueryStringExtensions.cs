using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;

namespace Dora.OAuthServer
{
    /// <summary>
    /// Define some extension methods to operates <see cref="IQueryCollection"/> and <see cref="IFormCollection"/>.
    /// </summary>
    public static class FormQueryStringExtensions
    {
        /// <summary>
        /// Get the value of specified parameter.
        /// </summary>
        /// <param name="query">The query string collection.</param>
        /// <param name="name">The name of parameter to get.</param>
        /// <returns>The value of parameter to get.</returns>
        /// <exception cref="ArgumentNullException">Specified <paramref name="query"/> is null.</exception>
        /// <exception cref="ArgumentNullException">Specified <paramref name="name"/> is null.</exception>
        /// <exception cref="ArgumentException">Specified <paramref name="name"/> is a white space string.</exception>
        public static string GetValue(this IQueryCollection query, string name)
        {
            return Guard.ArgumentNotNull(query, nameof(query)).TryGetValue(Guard.ArgumentNotNullOrWhiteSpace(name, nameof(name)), out StringValues values)
                ? values.ToString()
                : null;
        }

        /// <summary>
        /// Get the value of specified parameter.
        /// </summary>
        /// <param name="form">The sumit form.</param>
        /// <param name="name">The name of parameter to get.</param>
        /// <returns>The value of parameter to get.</returns>
        /// <exception cref="ArgumentNullException">Specified <paramref name="form"/> is null.</exception>
        /// <exception cref="ArgumentNullException">Specified <paramref name="name"/> is null.</exception>
        /// <exception cref="ArgumentException">Specified <paramref name="name"/> is a white space string.</exception>
        public static string GetValue(this IFormCollection form, string name)
        {
            return Guard.ArgumentNotNull(form, nameof(form)).TryGetValue(Guard.ArgumentNotNullOrWhiteSpace(name, nameof(name)), out StringValues values)
               ? values.ToString()
               : null;
        }
    }
}
