using System;
using System.Threading;
using System.Threading.Tasks;

namespace Dora.Interception.Test
{
    [AttributeUsage( AttributeTargets.Class| AttributeTargets.Method, AllowMultiple = true)]
    public class FakeInterceptorAttribute : InterceptorAttribute
    {
        public int Step { get; }
        public static string Result;

        public FakeInterceptorAttribute(int step = 1)
        {
            Step = step;
        }

        public static void Reset() => Result = "";
        public async Task InvokeAsync(InvocationContext invocationContext)
        {
            Result += Step.ToString();
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
