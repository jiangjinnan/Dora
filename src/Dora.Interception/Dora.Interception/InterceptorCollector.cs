using Dora.DynamicProxy;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Dora.Interception
{
    /// <summary>
    /// Default implementation of <see cref="IInterceptorCollector"/>.
    /// </summary>                                                   
    public class InterceptorCollector : IInterceptorCollector
    {
        private static HashSet<Type> _nonInterceptableTypes = new HashSet<Type>();

        /// <summary>
        /// Gets the builder.
        /// </summary>
        /// <value>
        /// The builder.
        /// </value>
        public IInterceptorChainBuilder Builder { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InterceptorCollector"/> class.
        /// </summary>
        /// <param name="builder">The builder.</param>
        public InterceptorCollector(IInterceptorChainBuilder builder)
        {
            this.Builder = Guard.ArgumentNotNull(builder, nameof(builder));
        }
        /// <summary>
        /// Gets the interceptors decorated with the type of target instance.
        /// </summary>
        /// <param name="typeToIntercept">The type to intercept.</param>
        /// <param name="targetType">Type of the target instance.</param>
        /// <returns>
        /// The <see cref="T:Dora.DynamicProxy.InterceptorDecoration" /> representing the type members decorated with interceptors.
        /// </returns>
        public InterceptorDecoration GetInterceptors(Type typeToIntercept, Type targetType)
        {
            Guard.ArgumentNotNull(typeToIntercept, nameof(typeToIntercept));
            Guard.ArgumentNotNull(targetType, nameof(targetType));

            InterceptorDecoration interceptors;

            if (_nonInterceptableTypes.Contains(typeToIntercept))
            {
                return InterceptorDecoration.Empty;
            }

            if (typeToIntercept == targetType)
            {
                interceptors = GetInterceptors(targetType);
                if (interceptors.MethodBasedInterceptors.Count == 0 && interceptors.PropertyBasedInterceptors.Count == 0)
                {
                    lock (_nonInterceptableTypes)
                    {
                        _nonInterceptableTypes.Add(typeToIntercept);
                    }
                }
                return interceptors;
            }

            if (!typeToIntercept.IsInterface)
            {
                return InterceptorDecoration.Empty;
            }

            if (CustomAttributeAccessor.GetCustomAttribute<NonInterceptableAttribute>(typeToIntercept, true) != null)
            {
                lock (_nonInterceptableTypes)
                {
                    _nonInterceptableTypes.Add(typeToIntercept);
                }
                return InterceptorDecoration.Empty;
            }

            var mapping = targetType.GetTypeInfo().GetRuntimeInterfaceMap(typeToIntercept);
            var srcProviders = this.ResolveInterceptorProviders(targetType);
            var providers = new Dictionary<MethodInfo, IInterceptorProvider[]>();
            foreach (var method in srcProviders.Keys)
            {
                var index = Array.IndexOf(mapping.TargetMethods, method);
                if (index > -1)
                {
                    providers.Add(mapping.InterfaceMethods[index], srcProviders[method]);
                }
            }
            var classProviders = CustomAttributeAccessor.GetCustomAttributes<IInterceptorProvider>(targetType, true).ToArray();
            interceptors =  this.GetInterceptors(typeToIntercept, providers);
            if (interceptors.MethodBasedInterceptors.Count == 0 && interceptors.PropertyBasedInterceptors.Count == 0)
            {
                lock (_nonInterceptableTypes)
                {
                    _nonInterceptableTypes.Add(typeToIntercept);
                }
            }
            return interceptors;
        }

        /// <summary>
        /// Gets the interceptors decorated with the specified type.
        /// </summary>
        /// <param name="typeToIntercept">The type to intercept.</param>
        /// <returns>
        /// The <see cref="T:Dora.DynamicProxy.InterceptorDecoration" /> representing the type members decorated with interceptors.
        /// </returns>
        public InterceptorDecoration GetInterceptors(Type typeToIntercept)
        {
            Guard.ArgumentNotNull(typeToIntercept, nameof(typeToIntercept));
            if (CustomAttributeAccessor.GetCustomAttribute<NonInterceptableAttribute>(typeToIntercept, true) != null)
            {
                return InterceptorDecoration.Empty;
            }

            var classProviders = CustomAttributeAccessor.GetCustomAttributes<IInterceptorProvider>(typeToIntercept, true).ToArray();
            var methodProviders = this.ResolveInterceptorProviders(typeToIntercept);
            return this.GetInterceptors(typeToIntercept, methodProviders);
        }

        private InterceptorDecoration GetInterceptors(Type typeToIntercept, Dictionary<MethodInfo, IInterceptorProvider[]> providers)
        {                                              
            var methodBasedDecorations = new List<MethodBasedInterceptorDecoration>();
            var propertyBasedDecorations = new List<PropertyBasedInterceptorDecoration>();
            foreach (var methodInfo in typeToIntercept.GetTypeInfo().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (methodInfo.IsSpecialName || methodInfo.DeclaringType == typeof(object))
                {
                    continue;
                }

                if (!providers.TryGetValue(methodInfo, out var methodProviders))
                {
                    continue;
                }
                var intercecptor = this.BuildInterceptor(methodProviders);
                methodBasedDecorations.Add(new MethodBasedInterceptorDecoration(methodInfo, intercecptor));
            }

            foreach (var property in typeToIntercept.GetTypeInfo().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {  
                InterceptorDelegate interceptorOfGetMethod = null;
                InterceptorDelegate interceptorOfSetMethod = null;
                var getMethod = property.GetMethod;
                var setMethod = property.SetMethod;
                if (null != getMethod && providers.TryGetValue(getMethod, out var getMethodProviders))
                {
                    interceptorOfGetMethod = this.BuildInterceptor(getMethodProviders);
                }

                if (null != setMethod && providers.TryGetValue(setMethod, out var setMethodProviders))
                {
                    interceptorOfSetMethod = this.BuildInterceptor(setMethodProviders);
                }

                if (null != interceptorOfGetMethod || null != interceptorOfSetMethod)
                {
                    propertyBasedDecorations.Add( new PropertyBasedInterceptorDecoration(property, interceptorOfGetMethod, interceptorOfSetMethod));
                }
            }
            return new InterceptorDecoration(methodBasedDecorations, propertyBasedDecorations);
        }
        private InterceptorDelegate BuildInterceptor(IInterceptorProvider[] providers)
        {
            var builder = this.Builder.New();
            foreach (var provider in providers)
            {
                provider.Use(builder);
            }
            return builder.Build();
        }
        private Dictionary<MethodInfo, IInterceptorProvider[]> ResolveInterceptorProviders(Type type)
        {
            var results =  new Dictionary<MethodInfo, IInterceptorProvider[]>();
            var nonInterceptableAttribute = CustomAttributeAccessor.GetCustomAttribute<NonInterceptableAttribute>(type, true);
            if (nonInterceptableAttribute != null && nonInterceptableAttribute.InterceptorProviderTypes.Length == 0)
            {
                return results;
            } 
            var classProviders = CustomAttributeAccessor.GetCustomAttributes<IInterceptorProvider>(type, true).ToArray();
            if (null != nonInterceptableAttribute)
            {
                classProviders = classProviders
                    .Where(it => !nonInterceptableAttribute.InterceptorProviderTypes.Contains(it.GetType()))
                    .ToArray();
            }   

            foreach (var methodInfo in type.GetTypeInfo().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (methodInfo.DeclaringType == typeof(object))
                {
                    continue;
                }

                if (methodInfo.IsSpecialName)
                {
                    continue;
                }

                nonInterceptableAttribute = CustomAttributeAccessor.GetCustomAttribute<NonInterceptableAttribute>(methodInfo, true);
                if (null != nonInterceptableAttribute && nonInterceptableAttribute.InterceptorProviderTypes.Length == 0)
                {
                    continue;
                }
                var providers = CustomAttributeAccessor.GetCustomAttributes<IInterceptorProvider>(methodInfo, true).ToArray();
                var list = new List<IInterceptorProvider>();
                list.AddRange(providers);
                foreach (var provider in classProviders)
                {
                    if (nonInterceptableAttribute?.InterceptorProviderTypes?.Contains(provider.GetType()) == true)
                    {
                        continue;
                    }

                    if (!provider.AllowMultiple && providers.Any(it => it.GetType() == provider.GetType()))
                    {
                        continue;
                    }
                    list.Add(provider);
                }

                if (list.Count > 0)
                {
                    results.Add(methodInfo, list.ToArray());
                }  
            } 

            foreach (var property in type.GetTypeInfo().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
               
                nonInterceptableAttribute = CustomAttributeAccessor.GetCustomAttribute<NonInterceptableAttribute>(property, true);
                if (null != nonInterceptableAttribute && nonInterceptableAttribute.InterceptorProviderTypes.Length == 0)
                {
                    continue;
                } 
                var propertyProviders = CustomAttributeAccessor.GetCustomAttributes<IInterceptorProvider>(property, true).ToArray();
                var list = new List<IInterceptorProvider>();
                list.AddRange(propertyProviders);
                foreach (var provider in classProviders)
                {
                    if (nonInterceptableAttribute?.InterceptorProviderTypes?.Contains(provider.GetType()) == true)
                    {
                        continue;
                    }

                    if (!provider.AllowMultiple && propertyProviders.Any(it => it.GetType() == provider.GetType()))
                    {
                        continue;
                    }
                    list.Add(provider);
                }

                var methods = new List<MethodInfo>();
                if (property.GetMethod != null)
                {
                    methods.Add(property.GetMethod);
                }
                if (property.SetMethod != null)
                {
                    methods.Add(property.SetMethod);
                }

                foreach (var method in methods)
                {
                    nonInterceptableAttribute = CustomAttributeAccessor.GetCustomAttribute<NonInterceptableAttribute>(method, true);
                    if (nonInterceptableAttribute != null && nonInterceptableAttribute.InterceptorProviderTypes.Length == 0)
                    {
                        continue;
                    }

                    var methodProviders = CustomAttributeAccessor.GetCustomAttributes<IInterceptorProvider>(method, true).ToArray();
                    var list1 = new List<IInterceptorProvider>();
                    list1.AddRange(methodProviders);

                    foreach (var provider in list)
                    {
                        if (nonInterceptableAttribute?.InterceptorProviderTypes?.Contains(provider.GetType()) == true)
                        {
                            continue;
                        }

                        if (!provider.AllowMultiple && methodProviders.Any(it => it.GetType() == provider.GetType()))
                        {
                            continue;
                        }
                        list1.Add(provider);
                    }

                    if (list1.Count > 0)
                    {
                        results[method] = list1.ToArray();
                    }
                } 
            }
            return results;
        } 
    }
}
