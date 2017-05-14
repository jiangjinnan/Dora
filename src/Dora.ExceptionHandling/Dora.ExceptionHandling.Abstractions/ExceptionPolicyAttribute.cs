using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.ExceptionHandling
{
    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage( AttributeTargets.Class| AttributeTargets.Method| AttributeTargets.Assembly)]
    public class ExceptionPolicyAttribute: Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        public string PolicyName { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="policyName"></param>
        public ExceptionPolicyAttribute(string policyName)
        {
            this.PolicyName = Guard.ArgumentNotNullOrWhiteSpace(policyName, nameof(policyName));
        }
    }
}
