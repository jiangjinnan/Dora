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
        private static MethodInfo _methodOfGetInterceptorForGetMethod;
        private static MethodInfo _methodOfGetInterceptorForSetMethod;

        public IDictionary<MethodInfo, MethodBasedInterceptorDecoration> MethodBasedInterceptors { get; }
        public IDictionary<PropertyInfo, PropertyBasedInterceptorDecoration>  PropertyBasedInterceptors { get; } 
        public InterceptorDecoration(
            IEnumerable<MethodBasedInterceptorDecoration> methodBasedInterceptors,
            IEnumerable<PropertyBasedInterceptorDecoration> propertyBasedInterceptors)
        {
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
                }
            }
        }

        public InterceptorDelegate GetInterceptor(MethodInfo methodInfo)
        {
            Guard.ArgumentNotNull(methodInfo, nameof(methodInfo));
            return this.MethodBasedInterceptors.TryGetValue(methodInfo, out var decoration)
                ? decoration.Interceptor
                : null;
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
        public static MethodInfo MethodOfGetInterceptorForGetMethod
        {
            get
            {
                return _methodOfGetInterceptorForGetMethod
                     ?? (_methodOfGetInterceptorForGetMethod = ReflectionUtility.GetMethod<InterceptorDecoration>(_ => _.GetInterceptorForGetMethod(null)));
            }
        }

        public static MethodInfo MethodOfSetInterceptorForGetMethod
        {
            get
            {
                return _methodOfGetInterceptorForSetMethod
                     ?? (_methodOfGetInterceptorForSetMethod = ReflectionUtility.GetMethod<InterceptorDecoration>(_ => _.GetInterceptorForSetMethod(null)));
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
                if (null == getMethod)
                {
                    throw new ArgumentException(Resources.ExceptionGetMethodNotExists.Fill(property.Name, property.DeclaringType.Name), nameof(property));
                }
                this.GetMethodBasedInterceptor = new MethodBasedInterceptorDecoration(getMethod, getMethodBasedInterceptor);
            }

            if (setMethodBasedInterceptor != null)
            {
                var setMethod = property.SetMethod;
                if (null == setMethod)
                {
                    throw new ArgumentException(Resources.ExceptionSetMethodNotExists.Fill(property.Name, property.DeclaringType.Name), nameof(property));
                }
                this.SetMethodBasedInterceptor = new MethodBasedInterceptorDecoration(setMethod, getMethodBasedInterceptor);
            }
        }
    }
}
