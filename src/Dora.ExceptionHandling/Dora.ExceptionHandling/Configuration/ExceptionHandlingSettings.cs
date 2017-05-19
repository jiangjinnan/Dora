using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.ExceptionHandling.Configuration
{
    internal class ExceptionHandlingSettings
    {
        public Dictionary<string, string> Alias { get; set; }
        public IEnumerable<PolicySection> Policies { get; set; }
    }
}
