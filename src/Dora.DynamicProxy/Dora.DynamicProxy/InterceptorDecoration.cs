using Dora.DynamicProxy.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Dora.DynamicProxy
{
    public class InterceptorDecoration
    {
        private static MethodInfo _methodOfGetInterceptor;
        private Dictionary<MethodInfo, InterceptorDelegate> _interceptors;

        public IDictionary<MethodInfo, MethodBasedInterceptorDecoration> MethodBasedInterceptors { get; }
        public IDictionary<PropertyInfo, PropertyBasedInterceptorDecoration>  PropertyBasedInterceptors { get; } 
        public InterceptorDecoration(
            IEnumerable<MethodBasedInterceptorDecoration> methodBasedInterceptors,
            IEnumerable<PropertyBasedInterceptorDecoration> propertyBasedInterceptors)
        {
            _interceptors = new Dictionary<MethodInfo, InterceptorDelegate>();
            this.MethodBasedInterceptors = new Dictionary<MethodInfo, MethodBasedInterceptorDecoration>();
            this.PropertyBasedInterceptors = new Dictionary<PropertyInfo, PropertyBasedInterceptorDecoration>();

            if (methodBasedInterceptors != null)
            {
                foreach (var it in methodBasedInterceptors)
                {
                    this.MethodBasedInterceptors[it.Method] = it; 
                }
            }

            if (propertyBasedInterceptors != null)
            {
                foreach (var it in propertyBasedInterceptors)
                {
                    this.PropertyBasedInterceptors[it.Property] = it;
                    var methodBasedInterceptor = it.GetMethodBasedInterceptor;
                }
            }

            _interceptors = (propertyBasedInterceptors?? new PropertyBasedInterceptorDecoration[0])
                .SelectMany(it => new MethodBasedInterceptorDecoration[] { it?.GetMethodBasedInterceptor, it?.SetMethodBasedInterceptor })
                .Union(methodBasedInterceptors?? new MethodBasedInterceptorDecoration[0])
                .Where(it => it != null)
                .ToDictionary(it => it.Method, it => it.Interceptor);
        }

        public InterceptorDelegate GetInterceptor(MethodInfo methodInfo)
        {
            Guard.ArgumentNotNull(methodInfo, nameof(methodInfo));
            return this._interceptors.TryGetValue(methodInfo, out var interceptor)
                ? interceptor
                : null;    
        }

        public bool IsInterceptable(MethodInfo methodInfo)
        {
            return _interceptors.ContainsKey(methodInfo);
        }

        public InterceptorDelegate GetInterceptorForGetMethod(PropertyInfo  propertyInfo)
        {
            Guard.ArgumentNotNull(propertyInfo, nameof(propertyInfo));
            return this.PropertyBasedInterceptors.TryGetValue(propertyInfo, out var decoration)
                ? decoration?.GetMethodBasedInterceptor.Interceptor
                : null;
        }    
        public InterceptorDelegate GetInterceptorForSetMethod(PropertyInfo propertyInfo)
        {
            Guard.ArgumentNotNull(propertyInfo, nameof(propertyInfo));
            return this.PropertyBasedInterceptors.TryGetValue(propertyInfo, out var decoration)
                ? decoration?.SetMethodBasedInterceptor.Interceptor
                : null;
        }

        public static MethodInfo MethodOfgetInterceptor
        {
            get
            {
                return _methodOfGetInterceptor
                     ?? (_methodOfGetInterceptor = ReflectionUtility.GetMethod<InterceptorDecoration>(_ => _.GetInterceptor(null)));
            }
        }
    }

    public class MethodBasedInterceptorDecoration
    {
        public MethodInfo Method { get;  }
        public InterceptorDelegate Interceptor { get; } 
        public MethodBasedInterceptorDecoration(MethodInfo method, InterceptorDelegate interceptor)
        {
            this.Method = Guard.ArgumentNotNull(method, nameof(method));
            this.Interceptor = Guard.ArgumentNotNull(interceptor, nameof(interceptor));
        }
    }

    public class PropertyBasedInterceptorDecoration
    {
        public PropertyInfo Property { get; }
        public MethodBasedInterceptorDecoration GetMethodBasedInterceptor { get; }   
        public MethodBasedInterceptorDecoration SetMethodBasedInterceptor { get; }
        public PropertyBasedInterceptorDecoration(
            PropertyInfo property,
            InterceptorDelegate getMethodBasedInterceptor,
            InterceptorDelegate setMethodBasedInterceptor)
        {
            this.Property = Guard.ArgumentNotNull(property, nameof(property));
            if (getMethodBasedInterceptor == null && setMethodBasedInterceptor == null)
            {
                throw new ArgumentException(Resources.ExceptionGetAndSetMethodBasedInterceptorCannotBeNull);
            }

            if (getMethodBasedInterceptor != null)
            {
                var getMethod = property.GetMethod;
                if (null != getMethod)
                {
                    this.GetMethodBasedInterceptor = new MethodBasedInterceptorDecoration(getMethod, getMethodBasedInterceptor);
                }
            }

            if (setMethodBasedInterceptor != null)
            {
                var setMethod = property.SetMethod;
                if (null != setMethod)
                {
                    this.SetMethodBasedInterceptor = new MethodBasedInterceptorDecoration(setMethod, setMethodBasedInterceptor);
                }
            }
        }
    }
}
