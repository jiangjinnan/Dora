using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.ExceptionHandling.Configuration
{
    internal class PolicySection
    {
        public string PolicyName { get; set; }
        public IEnumerable<PolicyEntrySection> PolicyEntries { get; set; }
        public IEnumerable<HandlerSection> PreHandlers { get; set; }
        public IEnumerable<HandlerSection> PostHandlers { get; set; }
    }
}
