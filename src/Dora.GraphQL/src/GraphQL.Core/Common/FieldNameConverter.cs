using System;

namespace Dora.GraphQL
{
    /// <summary>
    /// Represents a normalizer used to normailize field name.
    /// </summary>
    public class FieldNameConverter
    {
        private Func<string, string> _fromSource;
        private Func<string, string> _toDestination;
        private FieldNameConverter(Func<string, string> fromSource, Func<string, string> toDestination)
        {
            _fromSource = fromSource;
            _toDestination = toDestination;
        }

        /// <summary>
        /// Normalzes the specified original field name.
        /// </summary>
        /// <param name="originalFieldName">Name of the original field.</param>
        /// <param name="direction">The direction.</param>
        /// <returns>The normalized field name.</returns>
        public string Normalize(string originalFieldName, NormalizationDirection direction)
        {
            return direction == NormalizationDirection.Incoming
                ? _fromSource(originalFieldName)
                : _toDestination(originalFieldName);
        }

        /// <summary>
        /// The pascal case based <see cref="FieldNameConverter"/>.
        /// </summary>
        public static FieldNameConverter Default = new FieldNameConverter(_ => _, _ => _);

        /// <summary>
        /// The camel case based <see cref="FieldNameConverter"/>.
        /// </summary>
        public static FieldNameConverter CamelCase = new FieldNameConverter(src => ToPascalCase(src), src => ToCamelCase(src));

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
