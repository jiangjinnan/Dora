using System.Collections.Concurrent;
using System.Reflection;

namespace Dora.Interception.CodeGeneration
{
    /// <summary>
    /// Defines utility method to make generic method.
    /// </summary>
    public static class GenericMethodMaker
    {
        private static readonly ConcurrentDictionary<Tuple<MethodInfo, Type>, MethodInfo> _cache1 = new();
        private static readonly ConcurrentDictionary<Tuple<MethodInfo, Type, Type>, MethodInfo> _cache2 = new();
        private static readonly ConcurrentDictionary<Tuple<MethodInfo, Type, Type, Type>, MethodInfo> _cache3 = new();
        private static readonly ConcurrentDictionary<Tuple<MethodInfo, Type, Type, Type, Type>, MethodInfo> _cache4 = new();
        private static readonly ConcurrentDictionary<Tuple<MethodInfo, Type, Type, Type, Type, Type>, MethodInfo> _cache5 = new();
        private static readonly ConcurrentDictionary<Tuple<MethodInfo, Type, Type, Type, Type, Type, Type>, MethodInfo> _cache6 = new();

        /// <summary>
        /// Makes the generic method.
        /// </summary>
        /// <typeparam name="T">The 1st generic parameter type.</typeparam>
        /// <param name="definition">The generic method definition.</param>
        /// <returns>The made generic method.</returns>
        public static MethodInfo MakeGenericMethod<T>(MethodInfo definition)
        {
            var key = new Tuple<MethodInfo, Type>(definition, typeof(T));
            return _cache1.GetOrAdd(key, MakeGenericMethodCore);
            static MethodInfo MakeGenericMethodCore(Tuple<MethodInfo, Type> key) => key.Item1.MakeGenericMethod(key.Item2);
        }

        /// <summary>
        /// Makes the generic method.
        /// </summary>
        /// <typeparam name="T1">The type of the 1st generic parameter.</typeparam>
        /// <typeparam name="T2">The type of the 2nd generic parameter.</typeparam>
        /// <returns>The made generic method.</returns>
        public static MethodInfo MakeGenericMethod<T1, T2>(MethodInfo definition)
        {
            var key = new Tuple<MethodInfo, Type, Type>(definition, typeof(T1), typeof(T2));
            return _cache2.GetOrAdd(key, MakeGenericMethodCore);

            static MethodInfo MakeGenericMethodCore(Tuple<MethodInfo, Type, Type> key) => key.Item1.MakeGenericMethod(key.Item2, key.Item3);
        }

        /// <summary>
        /// Makes the generic method.
        /// </summary>
        /// <typeparam name="T1">The type of the 1st generic parameter.</typeparam>
        /// <typeparam name="T2">The type of the 2nd generic parameter.</typeparam>
        /// <typeparam name="T3">The type of the 3rd generic parameter.</typeparam>
        /// <returns>The made generic method.</returns>
        public static MethodInfo MakeGenericMethod<T1, T2, T3>(MethodInfo definition)
        {
            var key = new Tuple<MethodInfo, Type, Type, Type>(definition, typeof(T1), typeof(T2), typeof(T3));
            return _cache3.GetOrAdd(key, MakeGenericMethodCore);

            static MethodInfo MakeGenericMethodCore(Tuple<MethodInfo, Type, Type, Type> key) => key.Item1.MakeGenericMethod(key.Item2, key.Item3, key.Item4);
        }

        /// <summary>
        /// Makes the generic method.
        /// </summary>
        /// <typeparam name="T1">The type of the 1st generic parameter.</typeparam>
        /// <typeparam name="T2">The type of the 2nd generic parameter.</typeparam>
        /// <typeparam name="T3">The type of the 3rd generic parameter.</typeparam>
        /// <typeparam name="T4">The type of the 4th generic parameter.</typeparam>
        /// <returns>The made generic method.</returns>
        public static MethodInfo MakeGenericMethod<T1, T2, T3, T4>(MethodInfo definition)
        {
            var key = new Tuple<MethodInfo, Type, Type, Type, Type>(definition, typeof(T1), typeof(T2), typeof(T3), typeof(T4));
            return _cache4.GetOrAdd(key, MakeGenericMethodCore);

            static MethodInfo MakeGenericMethodCore(Tuple<MethodInfo, Type, Type, Type, Type> key) => key.Item1.MakeGenericMethod(key.Item2, key.Item3, key.Item4, key.Item5);
        }

        /// <summary>
        /// Makes the generic method.
        /// </summary>
        /// <typeparam name="T1">The type of the 1st generic parameter.</typeparam>
        /// <typeparam name="T2">The type of the 2nd generic parameter.</typeparam>
        /// <typeparam name="T3">The type of the 3rd generic parameter.</typeparam>
        /// <typeparam name="T4">The type of the 4th generic parameter.</typeparam>
        /// <typeparam name="T5">The type of the 5th generic parameter.</typeparam>
        /// <returns>The made generic method.</returns>
        public static MethodInfo MakeGenericMethod<T1, T2, T3, T4, T5>(MethodInfo definition)
        {
            var key = new Tuple<MethodInfo, Type, Type, Type, Type, Type>(definition, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
            return _cache5.GetOrAdd(key, MakeGenericMethodCore);

            static MethodInfo MakeGenericMethodCore(Tuple<MethodInfo, Type, Type, Type, Type, Type> key) => key.Item1.MakeGenericMethod(key.Item2, key.Item3, key.Item4, key.Item5, key.Item6);
        }

        /// <summary>
        /// Makes the generic method.
        /// </summary>
        /// <typeparam name="T1">The type of the 1st generic parameter.</typeparam>
        /// <typeparam name="T2">The type of the 2nd generic parameter.</typeparam>
        /// <typeparam name="T3">The type of the 3rd generic parameter.</typeparam>
        /// <typeparam name="T4">The type of the 4th generic parameter.</typeparam>
        /// <typeparam name="T5">The type of the 5th generic parameter.</typeparam>
        /// <typeparam name="T6">The type of the 6th generic parameter.</typeparam>
        /// <returns>The made generic method.</returns>
        public static MethodInfo MakeGenericMethod<T1, T2, T3, T4, T5, T6>(MethodInfo definition)
        {
            var key = new Tuple<MethodInfo, Type, Type, Type, Type, Type, Type>(definition, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6));
            return _cache6.GetOrAdd(key, MakeGenericMethodCore);

            static MethodInfo MakeGenericMethodCore(Tuple<MethodInfo, Type, Type, Type, Type, Type, Type> key) => key.Item1.MakeGenericMethod(key.Item2, key.Item3, key.Item4, key.Item5, key.Item6, key.Item7);
        }
    }
}
