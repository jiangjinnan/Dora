using Newtonsoft.Json;
using System;

namespace Dora.GraphQL.Executors
{
    /// <summary>
    /// Represent GraphQL query result.
    /// </summary>
    public struct  ExecutionResult
    {
        /// <summary>
        /// Gets or sets the payload.
        /// </summary>
        /// <value>
        /// The payload.
        /// </value>
        [JsonProperty(PropertyName = "data")]
        public object Data { get; set; }

        /// <summary>
        /// Gets or sets the error.
        /// </summary>
        /// <value>
        /// The error.
        /// </value>
        public string Error { get; set; }

        /// <summary>
        /// Create a new <see cref="ExecutionResult"/> of success status.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>The see cref="ExecutionResult"/> of success status.</returns>
        public static ExecutionResult Success(object data) => new ExecutionResult { Data = Guard.ArgumentNotNull(data, nameof(data)) };

        /// <summary>
        /// Create a new <see cref="ExecutionResult"/> of fail status.
        /// </summary>
        /// <param name="ex">The exception.</param>
        /// <returns>The see cref="ExecutionResult"/> of fail status.</returns>
        public static ExecutionResult Fail(Exception ex) => new ExecutionResult { Error = Guard.ArgumentNotNull(ex, nameof(ex)).Message };
    }
}
