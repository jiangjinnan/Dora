using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Dora.Interception
{
    internal static class InterceptorClassVerifier
    {
        public static void EnsureValidInterceptorClass(Type interceptorType)
        {
            if (typeof(IInterceptor).IsAssignableFrom(interceptorType))
            {
                return;
            }
            var invokeMethod = interceptorType.GetMethod("InvokeAsync", BindingFlags.Public | BindingFlags.Instance);
            if (invokeMethod == null || invokeMethod.ReturnType != typeof(Task) || !invokeMethod.GetParameters().Any(it => it.ParameterType == typeof(InvocationContext)))
            {
                throw new InvalidInterceptorDefintionException($"{interceptorType.FullName} is not a valid interceptor class with a public instance InvokeAsync method like 'public Task InvokeAsync(InvocationContext invocationContext, IFoo foo, IBar bar);'");
            }

            var captureArguments = interceptorType.GetProperty("CaptureArguments", BindingFlags.Public | BindingFlags.Instance);
            if (captureArguments != null && (captureArguments.GetMethod == null || captureArguments.PropertyType != typeof(bool)))
            {
                throw new InvalidInterceptorDefintionException($"{interceptorType.FullName} is not a valid interceptor class with a public instance CaptureArguments property of Boolean type.'");
            }
        }
    }
}
