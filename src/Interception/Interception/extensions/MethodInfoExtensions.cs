using System.Reflection;
using System.Threading.Tasks;

namespace Dora.Interception
{
    internal static class MethodInfoExtensions
    {
        public static MethodReturnKind GetReturnKind(this MethodInfo methodInfo)
        {
            var resultType = methodInfo.ReturnType;
            if (resultType == typeof(void))
            {
                return MethodReturnKind.Void;
            }

            if (resultType == typeof(Task))
            {
                return MethodReturnKind.Task;
            }

            if (resultType == typeof(ValueTask))
            {
                return MethodReturnKind.ValueTask;
            }

            if (resultType.IsGenericType)
            {
                if (resultType.GetGenericTypeDefinition() == typeof(Task<>))
                {
                    return MethodReturnKind.TaskOfResult;
                }
                if (resultType.GetGenericTypeDefinition() == typeof(ValueTask<>))
                {
                    return MethodReturnKind.ValueTaskOfResult;
                }
            }

            return MethodReturnKind.Result;
        }
    }
}
