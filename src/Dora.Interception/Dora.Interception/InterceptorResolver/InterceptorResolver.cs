using Dora.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Dora.Interception
{
    /// <summary>
    /// Default implementation of <see cref="IInterceptorResolver"/>.
    /// </summary>                                                   
    public class InterceptorResolver : IInterceptorResolver
    {
        #region Fields
        private  HashSet<Type> _nonInterceptableTypes = new HashSet<Type>();
        private CompositeInterceptorProviderResolver  _providerResolver;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the builder.
        /// </summary>
        /// <value>
        /// The builder.
        /// </value>
        public IInterceptorChainBuilder Builder { get; }
        # endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="InterceptorResolver" /> class.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="providerResolvers">The provider resolvers.</param>
        public InterceptorResolver(IInterceptorChainBuilder builder, IEnumerable< IInterceptorProviderResolver> providerResolvers)
        {
            Guard.ArgumentNotNull(builder, nameof(builder));
            Guard.ArgumentNotNullOrEmpty(providerResolvers, nameof(providerResolvers));

            _nonInterceptableTypes = new HashSet<Type>();
            _providerResolver = new  CompositeInterceptorProviderResolver(providerResolvers);
            this.Builder = builder;
        }
        #endregion

        #region Public Methods
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
            if (typeToIntercept.IsInterface)
            {   
                return this.GetInterceptorsCore(typeToIntercept, targetType, targetType.GetInterfaceMap(typeToIntercept));
            }

            return InterceptorDecoration.Empty;
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
            return this.GetInterceptorsCore(typeToIntercept);
        }
        #endregion

        #region Private Methods
        private InterceptorDecoration GetInterceptorsCore(Type typeToIntercept, Type targetType = null, InterfaceMapping? interfaceMapping = null)
        {
            Guard.ArgumentNotNull(typeToIntercept, nameof(typeToIntercept));
            var isInterface = interfaceMapping != null;
            if (_nonInterceptableTypes.Contains(typeToIntercept))
            {
                return InterceptorDecoration.Empty;
            }

            if (_providerResolver.WillIntercept(typeToIntercept) == false)
            {
                _nonInterceptableTypes.Add(typeToIntercept);
                return InterceptorDecoration.Empty;
            }

            var dictionary = new Dictionary<MethodInfo, InterceptorDelegate>();
            var providersOfType = _providerResolver.GetInterceptorProvidersForType(targetType?? typeToIntercept);
            providersOfType = this.SelectEffectiveProviders(providersOfType);

            var candidateMethods = this.GetCandidateMethods(typeToIntercept, isInterface);
            if (candidateMethods.Length > 0)
            {
                for (int index = 0; index < candidateMethods.Length; index++)
                {
                    var methodToIntercept = candidateMethods[index];
                    var targetMethod = this.GetTargetMethod(methodToIntercept, interfaceMapping);
                    var providersOfMethod = _providerResolver.GetInterceptorProvidersForMethod(targetMethod);
                    var providers = this.SelectEffectiveProviders(providersOfType, providersOfMethod);
                    if (providers.Length > 0)
                    {
                        dictionary[methodToIntercept] = this.BuildInterceptor(providers);
                    }
                }
            }  

            var candidateProperties = this.GetCandidateProperties(typeToIntercept, isInterface);

            for (int index = 0; index < candidateProperties.Length; index++)
            {
                var property = candidateProperties[index];
                var targetProperty = this.GetTargetProperty(property, targetType, interfaceMapping);
                var getMethod = property.GetMethod;
                var setMethod = property.SetMethod;
                if (null != getMethod && (isInterface || getMethod.IsVirtual))
                {    
                    var providersOfProperty = _providerResolver.GetInterceptorProvidersForProperty(targetProperty, PropertyMethod.Get);
                    var providers = this.SelectEffectiveProviders(providersOfType, providersOfProperty);
                    if (providers.Length > 0)
                    {
                        var builder = this.Builder.New();
                        Array.ForEach(providers, it => it.Use(builder));
                        dictionary[getMethod] = builder.Build();
                    }
                }

                if (null != setMethod && (isInterface || setMethod.IsVirtual))
                {
                    var providersOfProperty = _providerResolver.GetInterceptorProvidersForProperty(targetProperty, PropertyMethod.Set);
                    var providers = this.SelectEffectiveProviders(providersOfType, providersOfProperty);
                    if (providers.Length > 0 )
                    {
                        var builder = this.Builder.New();
                        Array.ForEach(providers, it => it.Use(builder));
                        dictionary[setMethod] = builder.Build();
                    }
                }  
            }    

            if (dictionary.Count == 0)
            {
                _nonInterceptableTypes.Add(typeToIntercept);
                return InterceptorDecoration.Empty;
            }

            return new InterceptorDecoration(dictionary, interfaceMapping);  
        }
        private MethodInfo[] GetCandidateMethods(Type type, bool isInterface)
        {
            return type
             .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
             .Where(it => !it.IsSpecialName && (isInterface || it.IsVirtual))
             .ToArray();
        }  
        private PropertyInfo[] GetCandidateProperties(Type type, bool isInterface)
        {
            return type
             .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
             .Where(it => isInterface || it?.GetMethod?.IsVirtual == true || it?.SetMethod?.IsVirtual == true)
             .ToArray();
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
        private IInterceptorProvider[] SelectEffectiveProviders(IInterceptorProvider[] typeBasedProviders)
        {
            var indicators = typeBasedProviders.ToDictionary(it => it, it => true);
            var dictionary = typeBasedProviders.GroupBy(it => it.GetType()).ToDictionary(it => it.Key, it => it.ToArray());
            foreach (var providerType in dictionary.Keys)
            {
                var providers = dictionary[providerType];
                if (!providers[0].AllowMultiple && providers.Length > 1)
                {
                    for (int index = 0; index < providers.Length - 1; index++)
                    {
                        indicators[providers[index]] = false;
                    }
                }
            }

            return typeBasedProviders
                .Where(it => indicators[it])
                .ToArray();
        } 
        private IInterceptorProvider[] SelectEffectiveProviders(IInterceptorProvider[] globalProviders, IInterceptorProvider[] specificProviders)
        {                                                                                
            var globalIndicators = globalProviders.ToDictionary(it => it, it => true);
            var specificIndicators = specificProviders.ToDictionary(it => it, it => true);

            var globaleDictionary = globalProviders.GroupBy(it => it.GetType()).ToDictionary(it=>it.Key, it=>it.ToArray());
            var specificDictionary = specificProviders.GroupBy(it => it.GetType()).ToDictionary(it => it.Key, it => it.ToArray());

            foreach (var providerType in specificDictionary.Keys)
            {
                var providers = specificDictionary[providerType];
                if (!providers[0].AllowMultiple)
                {
                    for (int index = 0; index < providers.Length-1; index++)
                    {
                        specificIndicators[providers[index]] = false;      
                    }

                    if (globaleDictionary.TryGetValue(providerType, out var typedProviders))
                    {
                        Array.ForEach(typedProviders, it=>globalIndicators[it] = false);
                    }
                }
            }

            return this.Concat(
                globalProviders.Where(it => globalIndicators[it]).ToArray(),
                specificProviders.Where(it => specificIndicators[it]).ToArray()); 
        }             
        private IInterceptorProvider[] Concat(IInterceptorProvider[] providers1, IInterceptorProvider[] providers2)
        {
            var lenghtOfproviders1 = providers1.Length;
            var array = new IInterceptorProvider[lenghtOfproviders1 + providers2.Length];
            for (int index = 0; index < providers1.Length; index++)
            {
                array[index] = providers1[index];
            }

            for (int index = 0; index < providers2.Length; index++)
            {
                array[index + lenghtOfproviders1] = providers2[index];
            }

            return array;
        }   
        private MethodInfo GetTargetMethod(MethodInfo methodToIntercept, InterfaceMapping? interfaceMapping = null)
        {
            if (interfaceMapping == null)
            {
                return methodToIntercept;
            }

            var index = Array.IndexOf(interfaceMapping.Value.InterfaceMethods, methodToIntercept);
            return interfaceMapping.Value.TargetMethods[index];
        }        
        private PropertyInfo GetTargetProperty(PropertyInfo propertyToIntercept, Type targetType = null, InterfaceMapping? interfaceMapping = null)
        {
            if (interfaceMapping == null)
            {
                return propertyToIntercept;
            }

            var getMethod = propertyToIntercept.GetMethod;
            if (null != getMethod)
            {
                var property = targetType
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .FirstOrDefault(it => it.GetMethod == this.GetTargetMethod(getMethod, interfaceMapping));
                if (null != property)
                {
                    return property;
                }
            }

            var setMethod = propertyToIntercept.SetMethod;
            if (null != getMethod)
            {
                var property = targetType
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .FirstOrDefault(it => it.SetMethod == this.GetTargetMethod(setMethod, interfaceMapping));
                if (null != property)
                {
                    return property;
                }
            }

            throw new InvalidOperationException();
        }
        #endregion
    }
}
