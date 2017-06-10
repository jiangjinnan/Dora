using System;

namespace Dora.ExceptionHandling
{
    /// <summary>
    /// Specify the name of exception policy applied to target class or method.
    /// </summary>
    [AttributeUsage( AttributeTargets.Class| AttributeTargets.Method| AttributeTargets.Assembly)]
    public class ExceptionPolicyAttribute: Attribute
    {
        /// <summary>
        /// The name of exception policy applied to target class or method.
        /// </summary>
        public string PolicyName { get; }

        /// <summary>
        /// Creates a new <see cref="ExceptionPolicyAttribute"/>.
        /// </summary>
        /// <param name="policyName">The name of exception policy applied to target class or method.</param>
        /// <exception cref="ArgumentNullException">The specified <paramref name="policyName"/> is null.</exception>
        /// <exception cref="ArgumentException">The specified <paramref name="policyName"/> is a white space string.</exception>
        public ExceptionPolicyAttribute(string policyName)
        {
            this.PolicyName = Guard.ArgumentNotNullOrWhiteSpace(policyName, nameof(policyName));
        }
    }
}
