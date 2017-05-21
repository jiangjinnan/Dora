using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Dora
{
    internal static class Guard
    {
        public static T ArgumentNotNull<T>(T argumentValue, string argumentName) where T : class
        {
            if (null == argumentValue)
            {
                throw new ArgumentNullException(argumentName, "The argument value cannot be null.");
            }
            return argumentValue;
        }

        public static string ArgumentNotNullOrEmpty(string argumentValue, string argumentName)
        {
            ArgumentNotNull(argumentValue, argumentName);
            if (string.IsNullOrEmpty(argumentValue))
            {
                throw new ArgumentException(argumentName, "The argument value cannot be empty.");
            }
            return argumentValue;
        }

        public static Guid ArgumentNotNullOrEmpty(Guid argumentValue, string argumentName)
        {
            if (argumentValue == Guid.Empty)
            {
                throw new ArgumentException(argumentName, "The argument value cannot be empty.");
            }
            return argumentValue;
        }

        public static IEnumerable<T> ArgumentNotNullOrEmpty<T>(IEnumerable<T> argumentValue, string argumentName)
        {
            ArgumentNotNull(argumentValue, argumentName);
            if (!argumentValue.Any())
            {
                throw new ArgumentException(argumentName, "The argument value cannot be an empty collection.");
            }
            return argumentValue;
        }

        public static string ArgumentNotNullOrWhiteSpace(string argumentValue, string argumentName)
        {
            ArgumentNotNull(argumentValue, argumentName);
            if (string.IsNullOrWhiteSpace(argumentValue))
            {
                throw new ArgumentException(argumentName, "The argument value cannot be a white space.");
            }

            return argumentValue;
        }

        public static Type ArgumentAssignableTo<T>(Type argumentValue, string argumentName)
        {
            Guard.ArgumentNotNull(argumentValue, argumentName);
            if (!typeof(T).GetTypeInfo().IsAssignableFrom(argumentValue))
            {
                throw new ArgumentException(argumentName, "The specified type \"{0}\"  cannot be assigned to the type \"{1}\".".Fill(argumentValue.FullName, typeof(T).FullName));
            }
            return argumentValue;
        }
    }
}
