using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Dora.DynamicProxy
{
    /// <summary>
    /// Represents the invocation context specific to calling the proxy.
    /// </summary>
    public abstract class InvocationContext
    {
        #region Fields
        private MethodBase _targetMethod;
        private Type _targetType;
        #endregion

        #region Properties
        /// <summary>
        ///  Gets the <see cref="MethodInfo"/> representing the method of type to intercept.
        /// </summary>
        public abstract MethodBase Method { get; }

        /// <summary>
        /// Gets the method of target type.
        /// </summary>
        /// <value>
        /// The method of target type.
        /// </value>
        public MethodBase TargetMethod
        {
            get
            {
                if (null != _targetMethod)
                {
                    return _targetMethod;
                }
                if (Method.DeclaringType.IsInterface)
                {
                    _targetType = _targetType ?? Target.GetType();
                    var map = _targetType.GetTypeInfo().GetRuntimeInterfaceMap(Method.DeclaringType);
                    var index = Array.IndexOf(map.InterfaceMethods, Method);
                    if (index > -1)
                    {
                        return _targetMethod = map.TargetMethods[index];
                    }
                }
                return _targetMethod = Method;
            }
        }

        /// <summary>
        /// Gets the proxy object on which the intercepted method is invoked.
        /// </summary>
        public abstract object Proxy { get; }

        /// <summary>
        /// Gets the object on which the invocation is performed.
        /// </summary>
        public abstract object Target { get; }

        /// <summary>
        /// Gets the arguments that target method has been invoked with.
        /// </summary>
        /// <remarks>Each argument is writable.</remarks>
        public abstract object[] Arguments { get; }

        /// <summary>
        /// Gets or sets the return value of the method.
        /// </summary>
        public abstract object ReturnValue { get; set; }  

        /// <summary>
        /// Gets the extended properties.
        /// </summary>
        /// <value>
        /// The extended properties.
        /// </value>
        public abstract IDictionary<string, object> ExtendedProperties { get; }

        /// <summary>
        /// Gets or sets the <see cref="InterceptDelegate"/> to invoke the next interceptor or target method.
        /// </summary>
        public InterceptDelegate Next { get; internal set; }
        #endregion

        #region Public Methods
        /// <summary>
        /// Invoke the next interceptor or target method.
        /// </summary>
        /// <returns>The task to invoke the next interceptor or target method.</returns>
        public Task ProceedAsync()
        {
            return Next(this);
        }
        #endregion
    }                             
}