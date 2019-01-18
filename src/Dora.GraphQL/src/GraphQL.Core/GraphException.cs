
using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.GraphQL
{

    [Serializable]
    public class GraphException : Exception
    {
        public GraphException() { }
        public GraphException(string message) : base(message) { }
        public GraphException(string message, Exception inner) : base(message, inner) { }
        protected GraphException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
