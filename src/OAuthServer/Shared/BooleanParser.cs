using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.OAuthServer
{
    internal static class BooleanParser
    {
        public static bool Parse(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            if (bool.TryParse(value, out var value1) && value1)
            {
                return true;
            }

            if (int.TryParse(value, out var value2) && value2 == 1)
            {
                return true;
            }

            return false;
        }
    }
}
