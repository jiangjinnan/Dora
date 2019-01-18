using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Dora.OAuthServer
{
    /// <summary>
    /// 
    /// </summary>
    public static class Guard
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="argumentValue"></param>
        /// <param name="argumentName"></param>
        /// <returns></returns>
        public static T ArgumentNotNull<T>(T argumentValue, string argumentName) where T : class
        {
            if (argumentValue == null)
            {
                throw new ArgumentNullException("Specified argument is null.", argumentName);
            }
            return argumentValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="argumentValue"></param>
        /// <param name="argumentName"></param>
        /// <returns></returns>
        public static string ArgumentNotNullOrWhiteSpace(string argumentValue, string argumentName)
        {
            ArgumentNotNull(argumentValue, argumentName);
            if (string.IsNullOrWhiteSpace(argumentValue))
            {
                throw new ArgumentException("Specified argument is a white space string.", argumentName);
            }
            return argumentValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="argumentValue"></param>
        /// <param name="argumentName"></param>
        /// <returns></returns>
        public static IEnumerable<T> ArgumentNotNullOrEmpty<T>(IEnumerable<T> argumentValue, string argumentName)
        {
            ArgumentNotNull(argumentValue, argumentName);
            if (!argumentValue.Any())
            {
                throw new ArgumentException("Specified argument is an empty collection.", argumentName);
            }
            return argumentValue;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="argumentValue"></param>
        /// <param name="argumentName"></param>
        /// <param name="allowEmpty"></param>
        /// <returns></returns>
        public static IEnumerable<string> ElementNotNullOrWhiteSpace(IEnumerable<string> argumentValue, string argumentName, bool allowEmpty = true)
        {
            ArgumentNotNull(argumentValue, argumentName);
            if (!allowEmpty)
            {
                ArgumentNotNullOrEmpty(argumentValue, argumentName);
            }
            if (argumentValue.Any() && argumentValue.Any(it => string.IsNullOrWhiteSpace(it)))
            {
                throw new ArgumentException("Some elements in specified collection are white space string.", argumentName);
            }
            return argumentValue;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="argumentValue"></param>
        /// <param name="argumentName"></param>
        /// <returns></returns>
        public static DateTimeOffset ArgumentMustBeUtc(DateTimeOffset argumentValue, string argumentName)
        {
            if (argumentValue.Offset.Ticks != 0)
            {
                throw new ArgumentException("pecified argument is not a valid UTC time.", argumentName);
            }
            return argumentValue;
        }
    }
}
