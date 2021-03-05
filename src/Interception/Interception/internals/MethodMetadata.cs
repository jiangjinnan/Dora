using System;
using System.Reflection;

namespace Dora.Interception
{
    public sealed class MethodMetadata
    {
        public MethodInfo MethodInfo { get; }
        public MethodReturnKind ReturnKind { get; }
        public bool IsGenericMethod { get; }
        public MethodMetadata(MethodInfo methodInfo)
        {
            MethodInfo = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));
            ReturnKind = methodInfo.GetReturnKind();
            IsGenericMethod = methodInfo.IsGenericMethod;
        }
    }
}
