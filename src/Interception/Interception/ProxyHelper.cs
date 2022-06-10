using System.Reflection;

namespace Dora.Interception
{
    /// <summary>
    /// The helper class used by dynamically generated interceptable proxy class.
    /// </summary>
    public static class ProxyHelper
    {
        private static readonly Dictionary<Tuple<Type, int>, MethodInfo> _methods = new();
        private static readonly BindingFlags _bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        /// <summary>
        /// Gets the argument or return value.
        /// </summary>
        /// <typeparam name="TAugument">The type of the augument.</typeparam>
        /// <typeparam name="TOutput">The type of the output.</typeparam>
        /// <param name="value">The value.</param>
        /// <returns>The argument or return value.</returns>
        public static TOutput GetArgumentOrReturnValue<TAugument, TOutput>(TAugument value)
        {
            if (value is TOutput result)
            {
                return result;
            }

            if (typeof(TOutput).IsAssignableFrom(typeof(TAugument)))
            {
                return default!;
            }

            throw new InvalidCastException($"Cannot cast the argument value from '{typeof(TAugument)}' to '{typeof(TOutput)}'");
        }

        /// <summary>
        /// Sets the argument or return value.
        /// </summary>
        /// <typeparam name="TInvocationContext">The type of the method invocation context.</typeparam>
        /// <typeparam name="TArgument">The type of the argument.</typeparam>
        /// <typeparam name="TInput">The type of the input.</typeparam>
        /// <param name="context">The method invocation context.</param>
        /// <param name="value">The argument or return value.</param>
        /// <param name="evaluate">The delegate to evaluate the raw argument or return value.</param>
        /// <returns>The specified methond invocation context.</returns>
        public static TInvocationContext SetArgumentOrReturnValue<TInvocationContext, TArgument, TInput>(TInvocationContext context, TInput value, Action<TInvocationContext, TArgument> evaluate) where TInvocationContext : InvocationContext
        {
            if (value is TArgument argument)
            {
                evaluate(context, argument);
                return context;
            }

            if (typeof(TArgument).IsAssignableFrom(typeof(TInput)))
            {
                evaluate(context, default!);
                return context;
            }

            throw new InvalidCastException($"Cannot cast the argument value from '{typeof(TInput)}' to '{typeof(TArgument)}'");
        }

        /// <summary>
        /// Gets the <see cref="MethodInfo"/> based on its metadata token.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="metadataToken">The metadata token of the <see cref="MethodInfo"/> to get.</param>
        /// <returns>The <see cref="MethodInfo"/> identified by specified metadata token.</returns>
        public static MethodInfo GetMethodInfo<T>(int metadataToken)
        {
            var key = new Tuple<Type, int>(typeof(T), metadataToken);
            return _methods.TryGetValue(key, out var method)
                ? method
                : _methods[key] = typeof(T).GetMethods(_bindingFlags).FirstOrDefault(it => it.MetadataToken == metadataToken) ?? throw new ArgumentException($"The method identified by specified token is not found in the type '{typeof(T)}'");
        }

        /// <summary>
        /// Converts specified <see cref="ValueTask{TResult}"/> as <see cref="ValueTask"/>.
        /// </summary>
        /// <typeparam name="TResult">The type of the result of specified <see cref="ValueTask{TResult}"/>.</typeparam>
        /// <param name="valueTask">The <see cref="ValueTask{TResult}"/> to convert.</param>
        /// <returns>The converted <see cref="ValueTask"/></returns>
        public static async ValueTask AsValueTask<TResult>(this ValueTask<TResult> valueTask)
        {
            if (!valueTask.IsCompleted)
            {
                await valueTask;
            }
        }
    }
}
