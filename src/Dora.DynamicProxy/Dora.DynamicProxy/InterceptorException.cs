using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.DynamicProxy
{
    /// <summary>
    /// Represents an InterceptorException.
    /// </summary>
    public class InterceptorException : Exception
    {
        /// <summary>
        /// 
        /// </summary>
        public bool RequireThrow { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        /// <param name="requireThrow"></param>
        public InterceptorException(string message, Exception innerException, bool requireThrow = true)
              : base(message, innerException)
        {
            RequireThrow = requireThrow;
        }
    }
}
