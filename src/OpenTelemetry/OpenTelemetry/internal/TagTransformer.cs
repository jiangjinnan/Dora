namespace Dora.OpenTelemetry
{
    internal abstract class TagTransformer<T>
    {
        public bool TryTransformTag(KeyValuePair<string, object?> tag, out T result)
        {
            if (tag.Value == null)
            {
                result = default!;
                return false;
            }

            switch (tag.Value)
            {
                case char:
                case string:
                    result = TransformStringTag(tag.Key, Convert.ToString(tag.Value)!);
                    break;
                case bool b:
                    result = TransformBooleanTag(tag.Key, b);
                    break;
                case byte:
                case sbyte:
                case short:
                case ushort:
                case int:
                case uint:
                case long:
                    result = TransformIntegralTag(tag.Key, Convert.ToInt64(tag.Value));
                    break;
                case float:
                case double:
                    result = TransformFloatingPointTag(tag.Key, Convert.ToDouble(tag.Value));
                    break;
                case Array array:
                    try
                    {
                        result = TransformArrayTagInternal(tag.Key, array);
                    }
                    catch
                    {
                        result = default!;
                        return false;
                    }

                    break;

                // All other types are converted to strings including the following
                // built-in value types:
                // case nint:    Pointer type.
                // case nuint:   Pointer type.
                // case ulong:   May throw an exception on overflow.
                // case decimal: Converting to double produces rounding errors.
                default:
                    try
                    {
                        result = TransformStringTag(tag.Key, tag.Value.ToString());
                    }
                    catch
                    {
                        result = default!;
                        return false;
                    }

                    break;
            }

            return true;
        }

        protected abstract T TransformIntegralTag(string key, long value);

        protected abstract T TransformFloatingPointTag(string key, double value);

        protected abstract T TransformBooleanTag(string key, bool value);

        protected abstract T TransformStringTag(string key, string value);

        protected abstract T TransformArrayTag(string key, Array array);

        private T TransformArrayTagInternal(string key, Array array)
        {
            // This switch ensures the values of the resultant array-valued tag are of the same type.
            return array switch
            {
                char[] => TransformArrayTag(key, array),
                string[] => TransformArrayTag(key, array),
                bool[] => TransformArrayTag(key, array),
                byte[] => TransformArrayTag(key, array),
                sbyte[] => TransformArrayTag(key, array),
                short[] => TransformArrayTag(key, array),
                ushort[] => TransformArrayTag(key, array),
                int[] => TransformArrayTag(key, array),
                uint[] => TransformArrayTag(key, array),
                long[] => TransformArrayTag(key, array),
                float[] => TransformArrayTag(key, array),
                double[] => TransformArrayTag(key, array),
                _ => ConvertToStringArrayThenTransformArrayTag(key, array),
            };
        }

        private T ConvertToStringArrayThenTransformArrayTag(string key, Array array)
        {
            var stringArray = new string[array.Length];

            for (var i = 0; i < array.Length; ++i)
            {
                stringArray[i] = array.GetValue(i)?.ToString()??"";
            }

            return TransformArrayTag(key, stringArray);
        }
    }
}
