using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dora.Interception.Test
{
    [AttributeUsage( AttributeTargets.Class| AttributeTargets.Method, AllowMultiple = true)]
    public class FakeInterceptorAttribute : InterceptorAttribute
    {
        private static readonly ConcurrentDictionary<Type, string> _results = new ConcurrentDictionary<Type, string>();

        public static string GetResult<T>()
        {
            var type = typeof(T);
            if (type.IsGenericType)
            {
                type = type.GetGenericTypeDefinition();
            }
            return _results.TryGetValue(type, out var value) ? value : "";
        }

        public int Step { get; }

        public FakeInterceptorAttribute(int step = 1)
        {
            Step = step;
        }

        public static void Reset<T>()
        {
            var type = typeof(T);
            if (type.IsGenericType)
            {
                type = type.GetGenericTypeDefinition();
            }
            _results[type] = "";
        }
        public async Task InvokeAsync(InvocationContext invocationContext)
        {
            var type = invocationContext.Method.DeclaringType;
            if (type.IsGenericType)
            {
                type = type.GetGenericTypeDefinition();
            }
            var result = _results.TryGetValue(type, out var value)
                ? value
                : _results[type] = "";
            result += Step.ToString();
            _results[type] = result;
            try
            {
                await invocationContext.ProceedAsync();
            }
            catch (Exception ex)
            {
                throw new FakeException(nameof(FakeException),ex);
            }
        }
        public override void Use(IInterceptorChainBuilder builder) => builder.Use(this, Order);
    }


    [Serializable]
    public class FakeException : Exception
    {
        public FakeException() { }
        public FakeException(string message) : base(message) { }
        public FakeException(string message, Exception inner) : base(message, inner) { }
        protected FakeException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
