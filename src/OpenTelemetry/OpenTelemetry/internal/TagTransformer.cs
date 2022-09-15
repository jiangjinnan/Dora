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
                    result = TransformString(tag.Key, Convert.ToString(tag.Value)!);
                    break;
                case bool b:
                    result = TransformBoolean(tag.Key, b);
                    break;
                case byte:
                case sbyte:
                case short:
                case ushort:
                case int:
                case uint:
                case long:
                    result = TransformInt64(tag.Key, Convert.ToInt64(tag.Value));
                    break;
                case float:
                case double:
                    result = TransformDouble(tag.Key, Convert.ToDouble(tag.Value));
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
                        result = TransformString(tag.Key, tag.Value?.ToString()??"");
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

        protected abstract T TransformInt64(string key, long value);

        protected abstract T TransformDouble(string key, double value);

        protected abstract T TransformBoolean(string key, bool value);

        protected abstract T TransformString(string key, string value);

        protected abstract T TransformArray(string key, Array array);

        private T TransformArrayTagInternal(string key, Array array)
        {
            // This switch ensures the values of the resultant array-valued tag are of the same type.
            return array switch
            {
                char[] => TransformArray(key, array),
                string[] => TransformArray(key, array),
                bool[] => TransformArray(key, array),
                byte[] => TransformArray(key, array),
                sbyte[] => TransformArray(key, array),
                short[] => TransformArray(key, array),
                ushort[] => TransformArray(key, array),
                int[] => TransformArray(key, array),
                uint[] => TransformArray(key, array),
                long[] => TransformArray(key, array),
                float[] => TransformArray(key, array),
                double[] => TransformArray(key, array),
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

            return TransformArray(key, stringArray);
        }
    }
}
