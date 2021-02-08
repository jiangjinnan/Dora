using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.Interception
{

    [Serializable]
    public class InvalidInterceptorDefintionException : Exception
    {
        public InvalidInterceptorDefintionException() { }
        public InvalidInterceptorDefintionException(string message) : base(message) { }
        public InvalidInterceptorDefintionException(string message, Exception inner) : base(message, inner) { }
        protected InvalidInterceptorDefintionException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
