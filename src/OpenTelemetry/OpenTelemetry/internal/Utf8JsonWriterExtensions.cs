using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace System.Text.Json
{
    internal static class Utf8JsonWriterExtensions
    {
        public static void WriteStringIfExists(this Utf8JsonWriter writer, JsonEncodedText propertyName, string? value)
        {
            if (value is not null)
            {
                writer.WriteString(propertyName, value);
            }
        }

        public static void WriteStringIfExists(this Utf8JsonWriter writer, string propertyName, string? value)
        {
            if (value is not null)
            {
                writer.WriteString(propertyName, value);
            }
        }

        public static void WriteNumberIfExists(this Utf8JsonWriter writer, JsonEncodedText propertyName, long? value)
        {
            if (value.HasValue)
            {
                writer.WriteNumber(propertyName, value.Value);
            }
        }

        public static void WriteNumberIfExists(this Utf8JsonWriter writer, JsonEncodedText propertyName, int? value)
        {
            if (value.HasValue)
            {
                writer.WriteNumber(propertyName, value.Value);
            }
        }

        public static void WriteBooleanIfExists(this Utf8JsonWriter writer, JsonEncodedText propertyName, bool? value)
        {
            if (value.HasValue)
            {
                writer.WriteBoolean(propertyName, value.Value);
            }
        }
    }
}
