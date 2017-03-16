using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Dora
{
  internal static class StringExtensions
  {
    public static string Fill(this string template, params object[] arguments)
    {
      Guard.ArgumentNotNullOrWhiteSpace(template, nameof(template));
      return string.Format(CultureInfo.CurrentCulture, template, arguments);
    }
  }
}
