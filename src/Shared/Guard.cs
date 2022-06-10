using System.Runtime.CompilerServices;

namespace Dora
{
    internal static class Guard
    {
        public static T ArgumentNotNull<T>(T value, [CallerArgumentExpression("value")] string? paramName = null) where T:class
        {
            if (value is null) throw new ArgumentNullException(paramName);
            return value;
        }

        public static string ArgumentNotNullOrWhitespace(string value, [CallerArgumentExpression("value")] string? paramName = null) 
        {
            if (value is null) throw new ArgumentNullException(paramName);
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Specified argument cannot be white space.", paramName);
            return value;
        }

        public static string ArgumentNotNullOrEmpty(string value, [CallerArgumentExpression("value")] string? paramName = null)
        {
            if (value is null) throw new ArgumentNullException(paramName);
            if (string.IsNullOrEmpty(value)) throw new ArgumentException("Specified argument cannot be white space.", paramName);
            return value;
        }
    }
}
