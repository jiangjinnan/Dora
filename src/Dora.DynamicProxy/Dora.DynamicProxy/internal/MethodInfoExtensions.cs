using System.Reflection;
using System.Threading.Tasks;

namespace Dora.DynamicProxy
{
    internal static class MethodInfoExtensions
    {
        public static bool ReturnVoid(this MethodInfo method)=> method.ReturnType == typeof(void);
        public static bool ReturnTask(this MethodInfo method)=> method.ReturnType == typeof(Task);
        public static bool ReturnTaskOfResult(this MethodInfo method)
        => method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>);
        public static bool IsOverridable(this MethodInfo method) => method.IsVirtual && !method.IsFinal;
    }
}
