using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Dora.Interception
{
    internal class MethodMetadata
    {
        public MethodInfo MethodInfo { get; }
        public MethodReturnKind ReturnKind { get; }

        public bool IsGenericMethod { get; }
        public MethodMetadata(MethodInfo methodInfo)
        {
            MethodInfo = methodInfo;
            ReturnKind = methodInfo.GetReturnKind();
            IsGenericMethod = methodInfo.IsGenericMethod;
        }
    }
}
