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
        public static FieldNameNormalizer CamelCase = new FieldNameNormalizer(src => ToPascalCase(src), src => ToCamelCase(src));


        private static string ToPascalCase(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || char.IsUpper(value[0]))
            {
                return value;
            }
            var array = value.ToCharArray();
            array[0] = char.ToUpper(array[0]);
            return new string(array);
        }

        private static string ToCamelCase(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || char.IsLower(value[0]))
            {
                return value;
            }
            var array = value.ToCharArray();
            array[0] = char.ToLower(array[0]);
            return new string(array);
        }
    }
}
