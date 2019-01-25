using System;

namespace Dora.GraphQL
{
    /// <summary>
    /// Represents a normalizer used to normailize field name.
    /// </summary>
    public class FieldNameConverter
    {
        private Func<string, string> _converter;
        private FieldNameConverter(Func<string, string> converter)
        {
            _converter = converter;
        }

        /// <summary>
        /// Normalzes the specified original field name.
        /// </summary>
        /// <param name="fieldName">Name of the original field.</param>
        /// <returns>The normalized field name.</returns>
        public string Normalize(string fieldName)
        {
            Guard.ArgumentNotNullOrWhiteSpace(fieldName, nameof(fieldName));
            return _converter(fieldName);
        }

        /// <summary>
        /// The pascal case based <see cref="FieldNameConverter"/>.
        /// </summary>
        public static FieldNameConverter Default = new FieldNameConverter(_ => _);

        /// <summary>
        /// The camel case based <see cref="FieldNameConverter"/>.
        /// </summary>
        public static FieldNameConverter CamelCase = new FieldNameConverter(src => ToCamelCase(src));

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
