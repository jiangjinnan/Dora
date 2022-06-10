using System.Reflection;

namespace App1
{
    internal class Key : IEquatable<Key>
    {
        public Key(MethodInfo method, IEnumerable<object> arguments)
        {
            Method = method;
            Arguments = arguments.ToArray();
        }

        public MethodInfo Method { get; }
        public object[] Arguments { get; }
        public bool Equals(Key? other)
        {
            if (other is null) return false;
            if (Method != other.Method) return false;
            if (Arguments.Length != other.Arguments.Length) return false;
            for (int index = 0; index < Arguments.Length; index++)
            {
                if (!Arguments[index].Equals(other.Arguments[index]))
                {
                    return false;
                }
            }
            return true;
        }
        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(Method);
            for (int index = 0; index < Arguments.Length; index++)
            {
                hashCode.Add(Arguments[index]);
            }
            return hashCode.ToHashCode();
        }
        public override bool Equals(object? obj) => obj is Key key && key.Equals(this);
    }
}
