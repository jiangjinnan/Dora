using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Dora.Interception
{
    /// <summary>
    /// Default implementation of <see cref="IInterceptorResolver"/>.
    /// </summary>                                                   
    public class InterceptorResolver : IInterceptorResolver
    {
        #region Fields
        private readonly Dictionary<Tuple<Type, Type>, IInterceptorRegistry> _instanceInteceptors;
        private readonly Dictionary<Type, IInterceptorRegistry> _typeInteceptors;
        private readonly ReaderWriterLockSlim _instanceLock;
        private readonly ReaderWriterLockSlim _typeLock;
        private readonly HashSet<Type> _nonInterceptableTypes = new HashSet<Type>();
        private readonly CompositeInterceptorProviderResolver _providerResolver;
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
            _instanceInteceptors = new Dictionary<Tuple<Type, Type>, IInterceptorRegistry>();
            _typeInteceptors = new Dictionary<Type, IInterceptorRegistry>();
            _instanceLock = new ReaderWriterLockSlim();
            _typeLock = new ReaderWriterLockSlim();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Gets the interceptors decorated with the type of target instance.
        /// </summary>
        /// <param name="interfaceType">The type to intercept.</param>
        /// <param name="targetType">Type of the target instance.</param>
        /// <returns>
        /// The <see cref="IInterceptorRegistry" /> representing the type members decorated with interceptors.
        /// </returns>
        public IInterceptorRegistry GetInterceptors(Type interfaceType, Type targetType)
        {
            Guard.ArgumentNotNull(interfaceType, nameof(interfaceType));
            Guard.ArgumentNotNull(targetType, nameof(targetType));

            var key = new Tuple<Type, Type>(interfaceType, targetType);
            _instanceLock.EnterReadLock();
            try
            {
                if (_instanceInteceptors.TryGetValue(key, out var interceptors))
                {
                    return interceptors;
                }
            }
            finally
            {
                _instanceLock.ExitReadLock();
            }

            _instanceLock.EnterWriteLock();
            try
            {
                if (!_instanceInteceptors.TryGetValue(key, out var interceptors))
                {
                    interceptors = CreateInterceptors();
                    _instanceInteceptors[key] = interceptors;
                }
                return interceptors;
            }
            finally
            {
                _instanceLock.ExitWriteLock();
            }

            IInterceptorRegistry CreateInterceptors()
            {
                if (interfaceType.IsInterface)
                {
                    InterfaceMethodMapping mapping;
                    try
                    {
                        if (interfaceType.IsGenericTypeDefinition)
                        {
                            mapping = ReflectionUtility.GetInterfaceMapForGenericTypeDefinition(interfaceType, targetType);
                        }
                        else
                        {
                            mapping = new InterfaceMethodMapping(targetType.GetInterfaceMap(interfaceType));
                        }
                    }
                    catch(Exception ex)
                    {
                        throw ex;
                        //return InterceptorRegistry.Empty;
                    }
                    return GetInterceptorsCore(interfaceType, targetType, mapping);
                }
                return InterceptorRegistry.Empty;
            }
        }

        /// <summary>
        /// Gets the interceptors decorated with the specified type.
        /// </summary>
        /// <param name="typeToIntercept">The type to intercept.</param>
        /// <returns>
        /// The <see cref="T:Dora.DynamicProxy.InterceptorDecoration" /> representing the type members decorated with interceptors.
        /// </returns>
        public IInterceptorRegistry GetInterceptors(Type typeToIntercept)
        {
            Guard.ArgumentNotNull(typeToIntercept, nameof(typeToIntercept));

            _typeLock.EnterReadLock();
            try
            {
                if (_typeInteceptors.TryGetValue(typeToIntercept, out var interceptors))
                {
                    return interceptors;
                }
            }
            finally
            {
                _typeLock.ExitReadLock();
            }

            _typeLock.EnterWriteLock();
            try
            {
                if (!_typeInteceptors.TryGetValue(typeToIntercept, out var interceptors))
                {
                    interceptors = GetInterceptorsCore(typeToIntercept, typeToIntercept);
                    _typeInteceptors[typeToIntercept] = interceptors;
                }
                return interceptors;
            }
            finally
            {
                _typeLock.ExitWriteLock();
            }
        }
        #endregion

        #region Private Methods
        private IInterceptorRegistry GetInterceptorsCore(Type typeToIntercept, Type targetType, InterfaceMethodMapping? interfaceMapping = null)
        {
            Guard.ArgumentNotNull(typeToIntercept, nameof(typeToIntercept));
            var execludedProviderTypes = new HashSet<Type>();
            targetType ??= typeToIntercept;
            var isInterface = interfaceMapping != null;
            if (_nonInterceptableTypes.Contains(typeToIntercept))
            {
                return InterceptorRegistry.Empty;
            }

            //If target type is explicitly marked as Non-interceptable.
            if (_providerResolver.WillIntercept(targetType) == false)
            {
                _nonInterceptableTypes.Add(typeToIntercept);
                return InterceptorRegistry.Empty;
            }

            var dictionary = new Dictionary<MethodInfo, InterceptorDelegate>();

            //Get all InterceptorProvider for target type.
            var providersOfType = _providerResolver.GetInterceptorProvidersForType(targetType, out var excluded4Type);
            foreach (var type in excluded4Type)
            {
                execludedProviderTypes.Add(type);
            }

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

                    if (_providerResolver.WillIntercept(targetType, targetMethod) == false)
                    {
                        continue;
                    }                        

                    var providersOfMethod = _providerResolver.GetInterceptorProvidersForMethod(targetType,targetMethod, out var execluded4Method);
                    foreach (var type in execluded4Method)
                    {
                        execludedProviderTypes.Add(type);
                    }
                    var providers = SelectEffectiveProviders(providersOfType, providersOfMethod, execludedProviderTypes);
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

                if (_providerResolver.WillIntercept(targetType, targetProperty) == false)
                {
                    continue;
                }

                var getMethod = property.GetMethod;
                var setMethod = property.SetMethod;

                //GET method
                if (null != getMethod && (isInterface || getMethod.IsOverridable()))
                {
                    var providersOfProperty = _providerResolver.GetInterceptorProvidersForProperty(targetType,targetProperty, PropertyMethod.Get, out var execluded4Get);
                    foreach (var type in execluded4Get)
                    {
                        execludedProviderTypes.Add(type);
                    }
                    var providers = SelectEffectiveProviders(providersOfType, providersOfProperty, execludedProviderTypes);
                    if (providers.Length > 0)
                    {
                        var builder = Builder.New();
                        Array.ForEach(providers, it => it.Use(builder));
                        dictionary[getMethod] = builder.Build();
                    }
                }

                if (null != setMethod && (isInterface || setMethod.IsOverridable()))
                {
                    var providersOfProperty = _providerResolver.GetInterceptorProvidersForProperty(targetType,targetProperty, PropertyMethod.Set, out var execued4Set);
                    foreach (var type in execued4Set)
                    {
                        execludedProviderTypes.Add(type);
                    }
                    var providers = SelectEffectiveProviders(providersOfType, providersOfProperty, execludedProviderTypes);
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
                return InterceptorRegistry.Empty;
            }

            return new InterceptorRegistry(dictionary, interfaceMapping);
        }
        private MethodInfo[] GetCandidateMethods(Type type, bool isInterface)
        {
            return type
             .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
             .Where(it => !it.IsSpecialName && (isInterface || it.IsOverridable()) && it.DeclaringType != typeof(object))
             .ToArray();
        }
        private PropertyInfo[] GetCandidateProperties(Type type, bool isInterface)
        {
            return type
             .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
             .Where(it => isInterface || it?.GetMethod?.IsOverridable() == true || it?.SetMethod?.IsOverridable() == true)
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
        private IInterceptorProvider[] SelectEffectiveProviders(IInterceptorProvider[] globalProviders, IInterceptorProvider[] specificProviders, HashSet<Type> execludedProviders)
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

            return Concat(globalProviders.Where(it => globalIndicators[it]).ToArray(), specificProviders.Where(it => specificIndicators[it]).ToArray())
                .Where(it => !execludedProviders.Contains(it.GetType()))
                .ToArray();
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
        private MethodInfo GetTargetMethod(MethodInfo methodToIntercept, InterfaceMethodMapping? interfaceMapping = null)
        {
            if (interfaceMapping == null)
            {
                return methodToIntercept;
            }

            var index = Array.IndexOf(interfaceMapping.Value.InterfaceMethods, methodToIntercept);
            return interfaceMapping.Value.TargetMethods[index];
        }
        private PropertyInfo GetTargetProperty(PropertyInfo propertyToIntercept, Type targetType = null, InterfaceMethodMapping? interfaceMapping = null)
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
