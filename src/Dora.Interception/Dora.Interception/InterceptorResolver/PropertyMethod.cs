using System;

namespace Dora.Interception
{
    /// <summary>
    /// Represents the method of property.
    /// </summary>
    [Flags]
    public enum PropertyMethod
    {
        /// <summary>
        /// The property's GET method
        /// </summary>
        Get = 1,

        /// <summary>
        /// The property's SET method
        /// </summary>
        Set = 2,

        /// <summary>
        /// The both
        /// </summary>
        Both = 3
    }
}
