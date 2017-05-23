using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.ExceptionHandling.Configuration
{
    internal class PolicyConfiguration
    {
        public List<PolicyEntryConfiguration> PolicyEntries { get;  }
        public List<HandlerConfiguration> PreHandlers { get; }
        public List<HandlerConfiguration> PostHandlers { get;  }
        public PolicyConfiguration()
        {
            this.PolicyEntries = new List<PolicyEntryConfiguration>();
            this.PreHandlers = new List<HandlerConfiguration>();
            this.PostHandlers = new List<HandlerConfiguration>();
        }
    }
}
