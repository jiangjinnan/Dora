using Dora.Interception;
using System.Threading.Tasks;

namespace Interception.Test
{
    public class FakeInterceptor: IInterceptor
    {
        public static InvocationContext InvocationContext { get; private set; }
        public static bool Invoked { get; set; }
        public bool CaptureArguments { get; }
        public object ReturnValue { get; }
        public InterceptorDelegate Delegate { get; }
        public FakeInterceptor(bool captureArguments, object returnValue )
        {
            Delegate  = next => context =>
            {
                context.Next = next;
                return InvokeAsync(context);
            };
            CaptureArguments = captureArguments;
            ReturnValue = returnValue;
        }

        public async Task InvokeAsync(InvocationContext invocationContext)
        {
            InvocationContext = invocationContext;
            await Task.Delay(10);
            await invocationContext.InvokeAsync();
            Invoked = true;
            if (null != ReturnValue)
            {
                invocationContext.SetReturnValue( ReturnValue);
            }
        }
        public static void Reset()
        {
            InvocationContext = null;
            Invoked = false;
        }
    }
}
