using System.Globalization;

namespace Dora.OAuthServer
{
    internal static class StringExtensions
    {
        public static string FormatMessage(this string template, params object[] arguments)
        {
            Guard.ArgumentNotNullOrWhiteSpace(template, nameof(template));
            return string.Format(CultureInfo.CurrentCulture, template, arguments);
        }
    }
}