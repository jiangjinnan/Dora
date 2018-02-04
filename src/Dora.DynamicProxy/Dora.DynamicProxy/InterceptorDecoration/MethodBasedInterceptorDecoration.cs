//using System;
//using System.Reflection;

//namespace Dora.DynamicProxy
//{
//    /// <summary>
//    /// Represents method based interceptor decoration.
//    /// </summary>
//    public class MethodBasedInterceptorDecoration
//    {
//        #region Properties
//        /// <summary>
//        /// Gets the method decorated with interceptor.
//        /// </summary>
//        /// <value>
//        /// The method decorated with interceptor.
//        /// </value>
//        public MethodInfo Method { get; }

//        /// <summary>
//        /// Gets the interceptor.
//        /// </summary>
//        /// <value>
//        /// The interceptor.
//        /// </value>
//        public InterceptorDelegate Interceptor { get; }
//        #endregion

//        #region Constructors

//        /// <summary>
//        /// Initializes a new instance of the <see cref="MethodBasedInterceptorDecoration"/> class.
//        /// </summary>
//        /// <param name="method">The method decorated with interceptor.</param>
//        /// <param name="interceptor">The interceptor.</param>          
//        /// <exception cref="ArgumentNullException"> Specified <paramref name="method"/> is null.</exception>
//        /// <exception cref="ArgumentNullException"> Specified <paramref name="interceptor"/> is null.</exception>
//        public MethodBasedInterceptorDecoration(MethodInfo method, InterceptorDelegate interceptor)
//        {
//            this.Method = Guard.ArgumentNotNull(method, nameof(method));
//            this.Interceptor = Guard.ArgumentNotNull(interceptor, nameof(interceptor));
//        }
//        #endregion
//    }
//}
