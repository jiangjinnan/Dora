using Dora.DynamicProxy.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Dora.DynamicProxy
{
    /// <summary>
    /// Representing which interceptors are applied to which members of a type to intercept.
    /// </summary>
    public class InterceptorDecoration
    {
        #region Fields
        private static readonly InterceptorDecoration _empty = new InterceptorDecoration(new MethodBasedInterceptorDecoration[0], new PropertyBasedInterceptorDecoration[0]);
        private static MethodInfo _methodOfGetInterceptor;
        private Dictionary<MethodInfo, InterceptorDelegate> _interceptors;
        private Dictionary<int, InterceptorDelegate> _tokenBasedInterceptors;
        #endregion

        #region Properties

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
        public bool IsEmpty { get => _interceptors.Count == 0; }

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
            return _interceptors.ContainsKey(Guard.ArgumentNotNull(methodInfo, nameof(methodInfo)));
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="InterceptorDecoration"/> class.
        /// </summary>
        /// <param name="methodBasedInterceptors">The method based interceptors.</param>
        /// <param name="propertyBasedInterceptors">The property based interceptors.</param>
        /// <exception cref="ArgumentNullException"> Specified <paramref name="methodBasedInterceptors"/> is null.</exception>    
        /// <exception cref="ArgumentNullException"> Specified <paramref name="propertyBasedInterceptors"/> is null.</exception>
        public InterceptorDecoration(
            IEnumerable<MethodBasedInterceptorDecoration> methodBasedInterceptors,
            IEnumerable<PropertyBasedInterceptorDecoration> propertyBasedInterceptors)
        {
            methodBasedInterceptors = methodBasedInterceptors ?? new MethodBasedInterceptorDecoration[0];
            propertyBasedInterceptors = propertyBasedInterceptors ?? new PropertyBasedInterceptorDecoration[0];

            _interceptors = new Dictionary<MethodInfo, InterceptorDelegate>();
            this.MethodBasedInterceptors = methodBasedInterceptors.ToDictionary(it => it.Method, it => it);
            this.PropertyBasedInterceptors = propertyBasedInterceptors.ToDictionary(it => it.Property, it => it);  

            _interceptors = (propertyBasedInterceptors?? new PropertyBasedInterceptorDecoration[0])
                .SelectMany(it => new MethodBasedInterceptorDecoration[] { it?.GetMethodBasedInterceptor, it?.SetMethodBasedInterceptor })
                .Union(methodBasedInterceptors?? new MethodBasedInterceptorDecoration[0])
                .Where(it => it != null)
                .ToDictionary(it => it.Method, it => it.Interceptor);
            _tokenBasedInterceptors = _interceptors.ToDictionary(it => it.Key.MetadataToken, it => it.Value);
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
            Guard.ArgumentNotNull(methodInfo, nameof(methodInfo));
            if (_interceptors.TryGetValue(methodInfo, out var interceptor))
            {
                return interceptor;
            }

            if (_tokenBasedInterceptors.TryGetValue(methodInfo.MetadataToken, out  interceptor))
            {
                return interceptor;
            }
            return null;  
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
            return _interceptors.ContainsKey(methodInfo);
        }

        /// <summary>
        /// Gets the interceptor decorated with the get method of specified proeprty.
        /// </summary>
        /// <param name="propertyInfo">The <see cref="PropertyInfo"/> representing the property whose get method is decorated with interceptor.</param>
        /// <returns>The interceptor decorated with the get method of specified proeprty.</returns> 
        /// <exception cref="ArgumentNullException"> Specified <paramref name="propertyInfo"/> is null.</exception>
        public InterceptorDelegate GetInterceptorForGetMethod(PropertyInfo  propertyInfo)
        {
            Guard.ArgumentNotNull(propertyInfo, nameof(propertyInfo));
            return this.PropertyBasedInterceptors.TryGetValue(propertyInfo, out var decoration)
                ? decoration?.GetMethodBasedInterceptor.Interceptor
                : null;
        }

        /// <summary>
        /// Gets the interceptor decorated with the set method of specified proeprty.
        /// </summary>
        /// <param name="propertyInfo">The <see cref="PropertyInfo"/> representing the property whose set method is decorated with interceptor.</param>
        /// <returns>The interceptor decorated with the set method of specified proeprty.</returns> 
        /// <exception cref="ArgumentNullException"> Specified <paramref name="propertyInfo"/> is null.</exception>
        public InterceptorDelegate GetInterceptorForSetMethod(PropertyInfo propertyInfo)
        {
            Guard.ArgumentNotNull(propertyInfo, nameof(propertyInfo));
            return this.PropertyBasedInterceptors.TryGetValue(propertyInfo, out var decoration)
                ? decoration?.SetMethodBasedInterceptor.Interceptor
                : null;
        }
        #endregion 
    }

    /// <summary>
    /// Represents method based interceptor decoration.
    /// </summary>
    public class MethodBasedInterceptorDecoration
    {
        /// <summary>
        /// Gets the method decorated with interceptor.
        /// </summary>
        /// <value>
        /// The method decorated with interceptor.
        /// </value>
        public MethodInfo Method { get;  }

        /// <summary>
        /// Gets the interceptor.
        /// </summary>
        /// <value>
        /// The interceptor.
        /// </value>
        public InterceptorDelegate Interceptor { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodBasedInterceptorDecoration"/> class.
        /// </summary>
        /// <param name="method">The method decorated with interceptor.</param>
        /// <param name="interceptor">The interceptor.</param>          
        /// <exception cref="ArgumentNullException"> Specified <paramref name="method"/> is null.</exception>
        /// <exception cref="ArgumentNullException"> Specified <paramref name="interceptor"/> is null.</exception>
        public MethodBasedInterceptorDecoration(MethodInfo method, InterceptorDelegate interceptor)
        {
            this.Method = Guard.ArgumentNotNull(method, nameof(method));
            this.Interceptor = Guard.ArgumentNotNull(interceptor, nameof(interceptor));
        }
    }

    /// <summary>
    /// Property based interceptor decoration.
    /// </summary>
    public class PropertyBasedInterceptorDecoration
    {
        /// <summary>
        /// Gets the property decorated with interceptor.
        /// </summary>
        /// <value>
        /// The property decorated with interceptor.
        /// </value>
        public PropertyInfo Property { get; }


        /// <summary>
        /// Gets interceptor decoration for property's get method.
        /// </summary>
        /// <value>
        /// The interceptor decoration for property's get method.
        /// </value>
        public MethodBasedInterceptorDecoration GetMethodBasedInterceptor { get; }

        /// <summary>
        /// Gets interceptor decoration for property's set method.
        /// </summary>
        /// <value>
        /// The interceptor decoration for property's set method.
        /// </value>
        public MethodBasedInterceptorDecoration SetMethodBasedInterceptor { get; }


        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyBasedInterceptorDecoration"/> class.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="getMethodBasedInterceptor">The get method based interceptor.</param>
        /// <param name="setMethodBasedInterceptor">The set method based interceptor.</param> 
        /// <exception cref="ArgumentNullException"> Specified <paramref name="property"/> is null.</exception>   
        /// <exception cref="ArgumentNullException"> Specified <paramref name="getMethodBasedInterceptor"/> and <paramref name="setMethodBasedInterceptor"/> are both null.</exception>
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
