using Dora.DynamicProxy.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Dora.DynamicProxy
{
    /// <summary>
    /// Representing which interceptors are applied to which members of a type to intercept.
    /// </summary>
    public sealed class InterceptorDecoration
    {
        #region Fields
        private static readonly InterceptorDecoration _empty = new InterceptorDecoration(new MethodBasedInterceptorDecoration[0], new PropertyBasedInterceptorDecoration[0]);
        private static MethodInfo _methodOfGetInterceptor;
        private Dictionary<MethodInfo, MethodInfo> _interfaceMapping;

        #endregion

        #region Properties

        /// <summary>
        /// The interceptors
        /// </summary>
        public IReadOnlyDictionary<int, InterceptorDelegate> Interceptors { get; }  

        /// <summary>
        /// Gets the method based interceptors.
        /// </summary>
        /// <value>
        /// The method based interceptors.
        /// </value>
        public IReadOnlyDictionary<MethodInfo, MethodBasedInterceptorDecoration> MethodBasedInterceptors { get; }


        /// <summary>
        /// Gets the property based interceptors.
        /// </summary>
        /// <value>
        /// The property based interceptors.
        /// </value>
        public IReadOnlyDictionary<PropertyInfo, PropertyBasedInterceptorDecoration>  PropertyBasedInterceptors { get; }

        /// <summary>
        /// Gets a value indicating whether there is no interceptor.
        /// </summary>
        /// <value>
        ///   <c>true</c> if no interceptor is applied.; otherwise, <c>false</c>.
        /// </value>
        public bool IsEmpty { get => Interceptors.Count == 0; }

        /// <summary>
        /// Gets an empty <see cref="InterceptorDecoration"/>.
        /// </summary>
        /// <value>
        /// The empty <see cref="InterceptorDecoration"/>.
        /// </value>
        public static InterceptorDecoration Empty { get => _empty; }

        internal static MethodInfo MethodOfGetInterceptor
        {
            get
            {
                return _methodOfGetInterceptor
                     ?? (_methodOfGetInterceptor = ReflectionUtility.GetMethod<InterceptorDecoration>(_ => _.GetInterceptor(null)));
            }
        }

        /// <summary>
        /// Determines whether [contains] [the specified method information].
        /// </summary>
        /// <param name="methodInfo">The method information.</param>
        /// <returns>
        ///   <c>true</c> if [contains] [the specified method information]; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(MethodInfo  methodInfo)
        {
            return Interceptors.ContainsKey(Guard.ArgumentNotNull(methodInfo, nameof(methodInfo)).MetadataToken);
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="InterceptorDecoration"/> class.
        /// </summary>
        /// <param name="methodBasedInterceptors">The method based interceptors.</param>
        /// <param name="propertyBasedInterceptors">The property based interceptors.</param>  
        /// <param name="interfaceMapping">Interface mapping.</param>
        /// <exception cref="ArgumentNullException"> Specified <paramref name="methodBasedInterceptors"/> is null.</exception>    
        /// <exception cref="ArgumentNullException"> Specified <paramref name="propertyBasedInterceptors"/> is null.</exception>
        public InterceptorDecoration(
            IEnumerable<MethodBasedInterceptorDecoration> methodBasedInterceptors,
            IEnumerable<PropertyBasedInterceptorDecoration> propertyBasedInterceptors,
            InterfaceMapping? interfaceMapping = null)
        {
            methodBasedInterceptors = methodBasedInterceptors ?? new MethodBasedInterceptorDecoration[0];
            propertyBasedInterceptors = propertyBasedInterceptors ?? new PropertyBasedInterceptorDecoration[0];

            Interceptors = new Dictionary<int, InterceptorDelegate>();
            this.MethodBasedInterceptors = methodBasedInterceptors.ToDictionary(it => it.Method, it => it);
            this.PropertyBasedInterceptors = propertyBasedInterceptors.ToDictionary(it => it.Property, it => it);  

            Interceptors = propertyBasedInterceptors
                .SelectMany(it => new MethodBasedInterceptorDecoration[] { it.GetMethodBasedInterceptor, it.SetMethodBasedInterceptor })
                .Union(methodBasedInterceptors)
                .Where(it => it != null)
                .ToDictionary(it => it.Method.MetadataToken, it => it.Interceptor);

            if (null != interfaceMapping)
            {
                _interfaceMapping = new Dictionary<MethodInfo, MethodInfo>();
                var interfaceMethods = interfaceMapping.Value.InterfaceMethods;
                var targetMethods = interfaceMapping.Value.TargetMethods;
                for (int index = 0; index < interfaceMethods.Length; index++)
                {
                    _interfaceMapping.Add(interfaceMethods[index], targetMethods[index]);
                }
            }                                     
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the interceptor based on specified method.
        /// </summary>
        /// <param name="methodInfo">The <see cref="MethodInfo"/> decorated with interceptor.</param>
        /// <returns>The <see cref="InterceptorDelegate"/> representing the interceptor decorated with specified method.</returns>    
        /// <exception cref="ArgumentNullException"> Specified <paramref name="methodInfo"/> is null.</exception>
        public InterceptorDelegate GetInterceptor(MethodInfo methodInfo)
        {
            return Interceptors.TryGetValue(methodInfo.MetadataToken, out var interceptor)
                ? interceptor
                : null;    
        }

        /// <summary>
        /// Determines whether the specified method information is interceptable.
        /// </summary>
        /// <param name="methodInfo">The method information.</param>
        /// <returns>
        ///   <c>true</c> if the specified method information is interceptable; otherwise, <c>false</c>.
        /// </returns>   
        /// <exception cref="ArgumentNullException"> Specified <paramref name="methodInfo"/> is null.</exception>
        public bool IsInterceptable(MethodInfo methodInfo)
        {
            Guard.ArgumentNotNull(methodInfo, nameof(methodInfo));
            return Interceptors.ContainsKey(methodInfo.MetadataToken);
        }

        /// <summary>
        /// Get target method.
        /// </summary>
        /// <param name="methodInfo">The method to intercept.</param>
        /// <returns>The target method.</returns>
        public MethodInfo GetTargetMethod(MethodInfo methodInfo)
        {
            Guard.ArgumentNotNull(methodInfo, nameof(methodInfo));
            if (_interfaceMapping == null)
            {
                return methodInfo;
            }
            return _interfaceMapping.TryGetValue(methodInfo, out var targetMethod)
                ? targetMethod
                : methodInfo;
        }
        #endregion 
    } 
}
