using System;

namespace Dora
{
    internal static class Guard
    {
        public static void ArgumentNotNull(object argumentValue, string argumentName)
        {
            if (null == argumentValue)
            {
                throw new ArgumentNullException(argumentName, "The argument value cannot be null.");
            }
        }

        public static void ArgumentNotNullOrEmpty(string argumentValue, string argumentName)
        {
            ArgumentNotNull(argumentValue, argumentName);
            if (string.IsNullOrEmpty(argumentValue))
            {
                throw new ArgumentNullException(argumentName, "The argument value cannot be empty.");
            }
        }

        public static void ArgumentNotNullOrWhiteSpace(string argumentValue, string argumentName)
        {
            ArgumentNotNull(argumentValue, argumentName);
            if (string.IsNullOrWhiteSpace(argumentValue))
            {
                throw new ArgumentNullException(argumentName, "The argument value cannot be a white space.");
            }
        }
    }
}
