using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.ExceptionHandling.Configuration
{
    internal class PolicyEntrySection
    {
        public string ExecptionType { get; set; }
        public IEnumerable<HandlerSection> Handlers {get;set;}
        public string PostHandlingAction { get; set; }
    }
}
