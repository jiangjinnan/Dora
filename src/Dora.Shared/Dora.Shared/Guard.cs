using System;

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
        throw new ArgumentNullException(argumentName, "The argument value cannot be empty.");
      }
      return argumentValue;
    }

    public static string ArgumentNotNullOrWhiteSpace(string argumentValue, string argumentName)
    {
      ArgumentNotNull(argumentValue, argumentName);
      if (string.IsNullOrWhiteSpace(argumentValue))
      {
        throw new ArgumentNullException(argumentName, "The argument value cannot be a white space.");
      }

      return argumentValue;
    }
  }
}
