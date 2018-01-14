using System.Collections.Generic;
using System.Reflection;

namespace Dora.DynamicProxy
{
    public class DefaultInvocationContext : InvocationContext
    {
        public override MethodBase Method { get; }          

        public override object Proxy { get; }

        public override object Target { get; }

        public override object[] Arguments { get; }

        public override object ReturnValue { get; set; }

        public IDictionary<string, object> ExtendedProperties { get; }

        public DefaultInvocationContext(
             MethodBase method,  
             object proxy,
             object target,
             object[] arguments)
        {
            this.Method = Guard.ArgumentNotNull(method, nameof(method));             
            this.Proxy = Guard.ArgumentNotNull(proxy, nameof(proxy));
            this.Target = Guard.ArgumentNotNull(target, nameof(target));
            this.Arguments = Guard.ArgumentNotNull(arguments, nameof(arguments));
            this.ExtendedProperties = new Dictionary<string, object>();
        }
    }
}
