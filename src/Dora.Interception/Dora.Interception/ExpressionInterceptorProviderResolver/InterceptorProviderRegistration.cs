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
            InterceptorProviderType = Guard.ArgumentNotNull(interceptorProviderType, nameof(interceptorProviderType));
            InterceptorProviderFactory = Guard.ArgumentNotNull(interceptorProviderFactory, nameof(interceptorProviderFactory));
            TargetRegistrations = Guard.ArgumentNotNullOrEmpty(targetRegistrations, nameof(targetRegistrations)).ToArray();
        } 
    }

    public class TargetRegistration
    {
        public Type TargetType { get; }           
        public bool? ExludedAllMembers { get; set; }
        public bool? IncludedAllMembers { get; set; }
        public ISet<int> IncludedMethods { get; }
        public ISet<int> ExludedMethods { get; }
        public IDictionary<int, PropertyMethod> IncludedProperties { get; }
        public IDictionary<int, PropertyMethod> ExludedProperties { get; }

        public TargetRegistration(Type targetType)
        {
            this.TargetType = Guard.ArgumentNotNull(targetType, nameof(targetType));
            this.IncludedMethods = new HashSet<int>();
            this.ExludedMethods = new HashSet<int>();
            this.IncludedProperties = new Dictionary<int, PropertyMethod>();
            this.ExludedProperties = new Dictionary<int, PropertyMethod>();
        }
    }
}
