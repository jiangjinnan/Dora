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
    /// 
    /// </summary>
    /// <seealso cref="Dora.DynamicProxy.IInterceptorCollector" />
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
            var methodProviders = new Dictionary<MethodInfo, IInterceptorProvider[]>();
            foreach (var method in srcProviders.Keys)
            {
                var index = Array.IndexOf(mapping.TargetMethods, method);
                if (index > -1)
                {
                    methodProviders.Add(mapping.InterfaceMethods[index], srcProviders[method]);
                }
            }
            var classProviders = CustomAttributeAccessor.GetCustomAttributes<IInterceptorProvider>(targetType, true).ToArray();
            interceptors =  this.GetInterceptors(classProviders, typeToIntercept, methodProviders);
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
            return this.GetInterceptors(classProviders, typeToIntercept, methodProviders);
        }
        private InterceptorDecoration GetInterceptors(IInterceptorProvider[] classProviders, Type typeToIntercept, Dictionary<MethodInfo, IInterceptorProvider[]> methodProviders)
        {                                              
            var methodBasedDecorations = new List<MethodBasedInterceptorDecoration>();
            var propertyBasedDecorations = new List<PropertyBasedInterceptorDecoration>();
            foreach (var methodInfo in typeToIntercept.GetTypeInfo().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (methodInfo.DeclaringType != typeToIntercept)
                {
                    continue;
                }
                var intercecptor = this.BuildInterceptor(classProviders, methodInfo, methodProviders);
                if (null != intercecptor)
                {
                    methodBasedDecorations.Add( new MethodBasedInterceptorDecoration(methodInfo, intercecptor));
                }
            }

            foreach (var property in typeToIntercept.GetTypeInfo().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (property.DeclaringType != typeToIntercept)
                {
                    continue;
                }
                InterceptorDelegate interceptorOfGetMethod = null;
                InterceptorDelegate interceptorOfSetMethod = null;
                var getMethod = property.GetMethod;
                var setMethod = property.SetMethod;
                if (null != getMethod)
                {
                    interceptorOfGetMethod = this.BuildInterceptor(classProviders, getMethod, methodProviders);
                }

                if (null != setMethod)
                {
                    interceptorOfSetMethod = this.BuildInterceptor(classProviders, setMethod, methodProviders);
                }

                if (null != interceptorOfGetMethod || null != interceptorOfSetMethod)
                {
                    propertyBasedDecorations.Add( new PropertyBasedInterceptorDecoration(property, interceptorOfGetMethod, interceptorOfSetMethod));
                }
            }
            return new InterceptorDecoration(methodBasedDecorations, propertyBasedDecorations);
        }
        private InterceptorDelegate BuildInterceptor(IInterceptorProvider[] classProviders, MethodInfo methodInfo, Dictionary<MethodInfo, IInterceptorProvider[]> methodProviders)
        {
            var providerTypes = new HashSet<Type>();
            var allProviders = new List<IInterceptorProvider>();
            if (methodInfo.IsPublic || methodInfo.IsFamily || methodInfo.IsFamilyOrAssembly)
            {
                if (methodProviders.TryGetValue(methodInfo, out var providers))
                {
                    allProviders.AddRange(providers);
                    Array.ForEach(providers, it => providerTypes.Add(it.GetType()));
                }
            }
            foreach (var provider in classProviders)
            {
                if (provider.AllowMultiple)
                {
                    providerTypes.Add(provider.GetType());
                    allProviders.Add(provider);
                    continue;
                }

                if (providerTypes.Add(provider.GetType()))
                {
                    allProviders.Add(provider);
                }
            }
            if (allProviders.Count > 0)
            {
                var builder = this.Builder.New();
                foreach (var provider in allProviders)
                {
                    provider.Use(builder);
                }
                return builder.Build();  
            }

            return null;
        }
        private Dictionary<MethodInfo, IInterceptorProvider[]> ResolveInterceptorProviders(Type type)
        {
            var results =  new Dictionary<MethodInfo, IInterceptorProvider[]>();
            var classNonInterceptableAttribute = CustomAttributeAccessor.GetCustomAttribute<NonInterceptableAttribute>(type, true);
            if (null != classNonInterceptableAttribute)
            {
                return results;
            }
            var classAttributes = CustomAttributeAccessor.GetCustomAttributes(type, true);
            foreach (var methodInfo in type.GetTypeInfo().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                var nonInterceptableAttribute = CustomAttributeAccessor.GetCustomAttribute<NonInterceptableAttribute>(methodInfo, true);
                if (null != nonInterceptableAttribute)
                {
                    continue;
                }
                var providers = CustomAttributeAccessor.GetCustomAttributes<IInterceptorProvider>(methodInfo, true).ToArray();    
                if (providers.Any())
                {
                    results.Add(methodInfo, providers);
                    var attributes = classAttributes.Union(CustomAttributeAccessor.GetCustomAttributes(methodInfo));
                    Array.ForEach(providers, it => (it as IAttributeCollection).AddRange(attributes));
                }
            } 
            foreach (var property in type.GetTypeInfo().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                var nonInterceptableAttribute = CustomAttributeAccessor.GetCustomAttribute<NonInterceptableAttribute>(property, true);
                if (null != nonInterceptableAttribute)
                {
                    continue;
                }
                var providers = CustomAttributeAccessor.GetCustomAttributes<IInterceptorProvider>(property, true).ToArray();                
                var getMethod = property.GetMethod;
                if (getMethod != null)
                {
                    nonInterceptableAttribute = CustomAttributeAccessor.GetCustomAttribute<NonInterceptableAttribute>(getMethod, true);
                    if (null == nonInterceptableAttribute)
                    {
                        var providers2 = CustomAttributeAccessor.GetCustomAttributes<IInterceptorProvider>(getMethod, true).ToArray();
                        if (providers.Length > 0 || providers2.Length > 0)
                        {
                            var allProviders = providers.Union(providers2).ToArray();
                            results.Add(getMethod, allProviders);
                            var attributes = classAttributes
                                .Union(CustomAttributeAccessor.GetCustomAttributes(property, true))
                                .Union(CustomAttributeAccessor.GetCustomAttributes(getMethod, true));
                            Array.ForEach(providers, it => (it as IAttributeCollection).AddRange(attributes));
                        }
                    }  
                }

                var setMethod = property.SetMethod;
                if (setMethod != null)
                {
                    nonInterceptableAttribute = CustomAttributeAccessor.GetCustomAttribute<NonInterceptableAttribute>(setMethod, true);
                    if (null == nonInterceptableAttribute)
                    {
                        var providers2 = CustomAttributeAccessor.GetCustomAttributes<IInterceptorProvider>(setMethod, true).ToArray();
                        if (providers.Length > 0 || providers2.Length > 0)
                        {
                            var allProviders = providers.Union(providers2).ToArray();
                            results.Add(setMethod, allProviders);
                            var attributes = classAttributes
                                .Union(CustomAttributeAccessor.GetCustomAttributes(property, true))
                                .Union(CustomAttributeAccessor.GetCustomAttributes(setMethod, true));
                            Array.ForEach(providers, it => (it as IAttributeCollection).AddRange(attributes));
                        }
                    }
                }

            }
            return results;
        } 
    }
}
