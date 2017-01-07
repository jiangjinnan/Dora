using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Dora.Interception
{
    [AttributeUsage( AttributeTargets.Class| AttributeTargets.Method,AllowMultiple = false)]
    public abstract class InterceptorAttribute : Attribute, IInterceptorProvider
    {
        private bool? _allowMultiple;
        public int Order { get; set; }
        public bool AllowMultiple
        {
           get
            {
                return _allowMultiple.HasValue
                   ? _allowMultiple.Value
                   : (_allowMultiple = this.GetType().GetTypeInfo().GetCustomAttribute<AttributeUsageAttribute>().AllowMultiple).Value;
            }
        }
        public abstract void Use(IInterceptorChainBuilder builder);
    }
}