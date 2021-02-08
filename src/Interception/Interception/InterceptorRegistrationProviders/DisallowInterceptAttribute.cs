using System;

namespace Dora.Interception
{
    [AttributeUsage( AttributeTargets.Class| AttributeTargets.Method| AttributeTargets.Property)]
    public sealed class DisallowInterceptAttribute: Attribute
    {
    }
}
