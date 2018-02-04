//using Dora.DynamicProxy.Properties;
//using System;
//using System.Reflection;

//namespace Dora.DynamicProxy
//{
//    /// <summary>
//    /// Property based interceptor decoration.
//    /// </summary>
//    public class PropertyBasedInterceptorDecoration
//    {
//        #region Properties
//        /// <summary>
//        /// Gets the property decorated with interceptor.
//        /// </summary>
//        /// <value>
//        /// The property decorated with interceptor.
//        /// </value>
//        public PropertyInfo Property { get; }

//        /// <summary>
//        /// Gets interceptor decoration for property's get method.
//        /// </summary>
//        /// <value>
//        /// The interceptor decoration for property's get method.
//        /// </value>
//        public MethodBasedInterceptorDecoration GetMethodBasedInterceptor { get; }

//        /// <summary>
//        /// Gets interceptor decoration for property's set method.
//        /// </summary>
//        /// <value>
//        /// The interceptor decoration for property's set method.
//        /// </value>
//        public MethodBasedInterceptorDecoration SetMethodBasedInterceptor { get; }

//        #endregion

//        #region Constructors

//        /// <summary>
//        /// Initializes a new instance of the <see cref="PropertyBasedInterceptorDecoration"/> class.
//        /// </summary>
//        /// <param name="property">The property.</param>
//        /// <param name="getMethodBasedInterceptor">The get method based interceptor.</param>
//        /// <param name="setMethodBasedInterceptor">The set method based interceptor.</param> 
//        /// <exception cref="ArgumentNullException"> Specified <paramref name="property"/> is null.</exception>   
//        /// <exception cref="ArgumentNullException"> Specified <paramref name="getMethodBasedInterceptor"/> and <paramref name="setMethodBasedInterceptor"/> are both null.</exception>
//        public PropertyBasedInterceptorDecoration(
//            PropertyInfo property,
//            InterceptorDelegate getMethodBasedInterceptor,
//            InterceptorDelegate setMethodBasedInterceptor)
//        {
//            this.Property = Guard.ArgumentNotNull(property, nameof(property));
//            if (getMethodBasedInterceptor == null && setMethodBasedInterceptor == null)
//            {
//                throw new ArgumentException(Resources.ExceptionGetAndSetMethodBasedInterceptorCannotBeNull);
//            }

//            if (getMethodBasedInterceptor != null)
//            {
//                var getMethod = property.GetMethod;
//                if (null != getMethod)
//                {
//                    this.GetMethodBasedInterceptor = new MethodBasedInterceptorDecoration(getMethod, getMethodBasedInterceptor);
//                }
//            }

//            if (setMethodBasedInterceptor != null)
//            {
//                var setMethod = property.SetMethod;
//                if (null != setMethod)
//                {
//                    this.SetMethodBasedInterceptor = new MethodBasedInterceptorDecoration(setMethod, setMethodBasedInterceptor);
//                }
//            }
//        }
//        #endregion
//    }
//}
