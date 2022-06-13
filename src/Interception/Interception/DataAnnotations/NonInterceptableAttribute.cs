namespace Dora.Interception
{
    /// <summary>
    /// Attribute used for interception suppression.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly|AttributeTargets.Method| AttributeTargets.Property| AttributeTargets.Class)]

    public class NonInterceptableAttribute: Attribute
    {
    }
}
