using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.OAuthServer
{

    [Serializable]
    public class OAuthException : Exception
    {
        public OAuthException() { }
        public OAuthException(string message) : base(message) { }
        public OAuthException(string message, Exception inner) : base(message, inner) { }
        protected OAuthException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
