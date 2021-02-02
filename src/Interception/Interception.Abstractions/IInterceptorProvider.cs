using System.Reflection;

namespace Dora.Interception
{
    public interface IInterceptorProvider
    {
        IInterceptor GetInterceptor(MethodInfo method);
    }
}
