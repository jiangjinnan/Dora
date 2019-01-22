using System;
using System.Globalization;

namespace Dora.GraphQL
{
    public  class FieldNameNormalizer
    {
        private Func<string, string> _fromSource;
        private Func<string, string> _toDestination;
        private FieldNameNormalizer(Func<string, string> fromSource, Func<string, string> toDestination)
        {
            _fromSource = fromSource;
            _toDestination = toDestination;
        }
        public string NormalizeFromSource(string fieldName) => _fromSource(fieldName);
        public string NormalizeToDestination(string fieldName) => _toDestination(fieldName);

        public static FieldNameNormalizer PascalCase = new FieldNameNormalizer(_ => _, _ => _);
        public static FieldNameNormalizer Default = new FieldNameNormalizer(_ => _, _ => _);
        public static FieldNameNormalizer CamelCase = new FieldNameNormalizer(src => ToPascalOrCamel(src, true), src => ToPascalOrCamel(src, false));

        public static string ToPascalOrCamel(string s, bool pascal)
        {
            if (string.IsNullOrEmpty(s) || !char.IsUpper(s[0]))
            {
                return s;
            }
            char[] chArray = s.ToCharArray();
            for (int i = 0; i < chArray.Length; i++)
            {
                if ((i == 1) && !char.IsUpper(chArray[i]))
                {
                    break;
                }
                bool flag = (i + 1) < chArray.Length;
                if (((i > 0) & flag) && !char.IsUpper(chArray[i + 1]))
                {
                    if (char.IsSeparator(chArray[i + 1]))
                    {
                        chArray[i] = ToLowerOrUpper(chArray[i], pascal);
                    }
                    break;
                }
                chArray[i] = ToLowerOrUpper(chArray[i], pascal);
            }
            return (string)new string(chArray);
        }

        private static char ToLowerOrUpper(char c, bool upper)
        {
            return upper
                ? char.ToUpper(c, CultureInfo.InvariantCulture)
                : char.ToLower(c, CultureInfo.InvariantCulture);
        }
    }
}
