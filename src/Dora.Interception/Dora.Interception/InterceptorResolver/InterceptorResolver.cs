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
        private HashSet<Type> _nonInterceptableTypes = new HashSet<Type>();
        private CompositeInterceptorProviderResolver _providerResolver;
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
        public InterceptorResolver(IInterceptorChainBuilder builder, IEnumerable<IInterceptorProviderResolver> providerResolvers)
        {
            Guard.ArgumentNotNull(builder, nameof(builder));
            Guard.ArgumentNotNullOrEmpty(providerResolvers, nameof(providerResolvers));

            _nonInterceptableTypes = new HashSet<Type>();
            _providerResolver = new CompositeInterceptorProviderResolver(providerResolvers);
            Builder = builder;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Gets the interceptors decorated with the type of target instance.
        /// </summary>
        /// <param name="interfaceType">The type to intercept.</param>
        /// <param name="targetType">Type of the target instance.</param>
        /// <returns>
        /// The <see cref="T:Dora.DynamicProxy.InterceptorDecoration" /> representing the type members decorated with interceptors.
        /// </returns>
        public InterceptorDecoration GetInterceptors(Type interfaceType, Type targetType)
        {
            Guard.ArgumentNotNull(interfaceType, nameof(interfaceType));
            Guard.ArgumentNotNull(targetType, nameof(targetType));
            if (interfaceType.IsInterface)
            {
                return GetInterceptorsCore(interfaceType, targetType, targetType.GetInterfaceMap(interfaceType));
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
            return GetInterceptorsCore(typeToIntercept, typeToIntercept);
        }
        #endregion

        #region Private Methods
        private InterceptorDecoration GetInterceptorsCore(Type typeToIntercept, Type targetType, InterfaceMapping? interfaceMapping = null)
        {
            Guard.ArgumentNotNull(typeToIntercept, nameof(typeToIntercept));
            targetType = targetType ?? typeToIntercept;
            var isInterface = interfaceMapping != null;
            if (_nonInterceptableTypes.Contains(typeToIntercept))
            {
                return InterceptorDecoration.Empty;
            }

            //If target type is explicitly marked as Non-interceptable.
            if (_providerResolver.WillIntercept(targetType) == false)
            {
                _nonInterceptableTypes.Add(typeToIntercept);
                return InterceptorDecoration.Empty;
            }

            var dictionary = new Dictionary<MethodInfo, InterceptorDelegate>();

            //Get all InterceptorProvider for target type.
            var providersOfType = _providerResolver.GetInterceptorProvidersForType(targetType);   

            //Filter based on AllowMultiple property
            providersOfType = SelectEffectiveProviders(providersOfType);       

            //Resolve method based interceptor providers
            var candidateMethods = GetCandidateMethods(typeToIntercept, isInterface);
            if (candidateMethods.Length > 0)
            {
                for (int index = 0; index < candidateMethods.Length; index++)
                {
                    var methodToIntercept = candidateMethods[index];
                    var targetMethod = GetTargetMethod(methodToIntercept, interfaceMapping);
                    var providersOfMethod = _providerResolver.GetInterceptorProvidersForMethod(targetType,targetMethod);
                    var providers = SelectEffectiveProviders(providersOfType, providersOfMethod);
                    if (providers.Length > 0)
                    {
                        var builder = Builder.New();
                        Array.ForEach(providers, it => it.Use(builder));
                        dictionary[methodToIntercept] = builder.Build();
                    }
                }
            }

            //Resolve prooerty based interceptor providers
            var candidateProperties = GetCandidateProperties(typeToIntercept, isInterface);     
            for (int index = 0; index < candidateProperties.Length; index++)
            {
                var property = candidateProperties[index];
                var targetProperty = GetTargetProperty(property, targetType, interfaceMapping);
                var getMethod = property.GetMethod;
                var setMethod = property.SetMethod;

                //GET method
                if (null != getMethod && (isInterface || getMethod.IsVirtual))
                {
                    var providersOfProperty = _providerResolver.GetInterceptorProvidersForProperty(targetType,targetProperty, PropertyMethod.Get);
                    var providers = SelectEffectiveProviders(providersOfType, providersOfProperty);
                    if (providers.Length > 0)
                    {
                        var builder = Builder.New();
                        Array.ForEach(providers, it => it.Use(builder));
                        dictionary[getMethod] = builder.Build();
                    }
                }

                if (null != setMethod && (isInterface || setMethod.IsVirtual))
                {
                    var providersOfProperty = _providerResolver.GetInterceptorProvidersForProperty(targetType,targetProperty, PropertyMethod.Set);
                    var providers = SelectEffectiveProviders(providersOfType, providersOfProperty);
                    if (providers.Length > 0)
                    {
                        var builder = Builder.New();
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
       
        private IInterceptorProvider[] SelectEffectiveProviders(IInterceptorProvider[] typeBasedProviders)
        {
            //For interceptor providers of the same type, only keep the last one if not AllowMultiple.
            var indicators = typeBasedProviders.ToDictionary(it => it, it => true);
            var dictionary = typeBasedProviders
                .GroupBy(it => it.GetType())
                .ToDictionary(it => it.Key, it => it.ToArray());
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
            //For interceptor providers of the same type, only keep the last one if not AllowMultiple.
            var globalIndicators = globalProviders.ToDictionary(it => it, it => true);
            var specificIndicators = specificProviders.ToDictionary(it => it, it => true);

            var globalDictionary = globalProviders.GroupBy(it => it.GetType()).ToDictionary(it => it.Key, it => it.ToArray());
            var specificDictionary = specificProviders.GroupBy(it => it.GetType()).ToDictionary(it => it.Key, it => it.ToArray());

            foreach (var providerType in specificDictionary.Keys)
            {
                var providers = specificDictionary[providerType];
                //Keep the latest one
                if (!providers[0].AllowMultiple)
                {
                    for (int index = 0; index < providers.Length - 1; index++)
                    {
                        specificIndicators[providers[index]] = false;
                    }

                    if (globalDictionary.TryGetValue(providerType, out var typedProviders))
                    {
                        Array.ForEach(typedProviders, it => globalIndicators[it] = false);
                    }
                }
            }

            return Concat(
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
                    .FirstOrDefault(it => it.GetMethod == GetTargetMethod(getMethod, interfaceMapping));
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
                    .FirstOrDefault(it => it.SetMethod == GetTargetMethod(setMethod, interfaceMapping));
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
