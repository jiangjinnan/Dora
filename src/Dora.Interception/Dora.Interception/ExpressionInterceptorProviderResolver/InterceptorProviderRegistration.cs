using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Dora.Interception
{
    public class InterceptorProviderRegistration
    {
        public Type InterceptorProviderType { get; }
        public Func<IInterceptorProvider> InterceptorProviderFactory { get; }
        public TargetRegistration[] TargetRegistrations { get; }  
        public InterceptorProviderRegistration( 
           Type interceptorProviderType,
           Func<IInterceptorProvider> interceptorProviderFactory,
           IEnumerable< TargetRegistration> targetRegistrations)
        {
            this.InterceptorProviderType = Guard.ArgumentNotNull(interceptorProviderType, nameof(interceptorProviderType));
            this.InterceptorProviderFactory = Guard.ArgumentNotNull(interceptorProviderFactory, nameof(interceptorProviderFactory));
            this.TargetRegistrations = Guard.ArgumentNotNullOrEmpty(targetRegistrations, nameof(targetRegistrations)).ToArray();
        } 
    }

    public class TargetRegistration
    {
        public Type TargetType { get; }           
        public bool? ExludedAllMembers { get; set; }
        public bool? IncludedAllMembers { get; set; }
        public ISet<MethodInfo> IncludedMethods { get; }
        public ISet<MethodInfo> ExludedMethods { get; }
        public IDictionary<PropertyInfo, PropertyMethod> IncludedProperties { get; }
        public IDictionary<PropertyInfo, PropertyMethod> ExludedProperties { get; }

        public TargetRegistration(Type targetType)
        {
            this.TargetType = Guard.ArgumentNotNull(targetType, nameof(targetType));
            this.IncludedMethods = new HashSet<MethodInfo>();
            this.ExludedMethods = new HashSet<MethodInfo>();
            this.IncludedProperties = new Dictionary<PropertyInfo, PropertyMethod>();
            this.ExludedProperties = new Dictionary<PropertyInfo, PropertyMethod>();
        }
    }
}
