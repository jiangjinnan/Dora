using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.ExceptionHandling.Configuration
{
    internal class ExceptionPolicyElement
    {
        public List<PolicyEntryElement> PolicyEntries { get;  }
        public List<ExceptionHandlerElement> PreHandlers { get; }
        public List<ExceptionHandlerElement> PostHandlers { get;  }
        public ExceptionPolicyElement()
        {
            this.PolicyEntries = new List<PolicyEntryElement>();
            this.PreHandlers = new List<ExceptionHandlerElement>();
            this.PostHandlers = new List<ExceptionHandlerElement>();
        }
    }
}
