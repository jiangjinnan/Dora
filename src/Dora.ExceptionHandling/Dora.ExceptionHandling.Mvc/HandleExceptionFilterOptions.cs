using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.ExceptionHandling.Mvc
{
    /// <summary>
    /// The <see cref="HandleExceptionAttribute"/> based configuration options.
    /// </summary>
    public class HandleExceptionFilterOptions
    {
        /// <summary>
        /// The name of exception policy.
        /// </summary>
        public string ExceptionPolicy { get; set; }

        /// <summary>
        /// The name of error view. The default value is "Error".
        /// </summary>
        public string ErrorViewName { get; set; }

        /// <summary>
        /// A <see cref="Func{String, String}"/> to resolve handler action based on specified action name. 
        /// </summary>
        public Func<string, string> ResolveHandlerAction { get; set; }

        /// <summary>
        /// Indicates whether to include inner exception information. The default value is "false".
        /// </summary>
        public bool IncludeInnerException { get; set; }

        /// <summary>
        /// A <see cref="JsonSerializerSettings"/> used for JSON based serialization.
        /// </summary>
        public JsonSerializerSettings JsonSerializerSettings { get; set; }

        /// <summary>
        /// Creates a new <see cref="HandleExceptionFilterOptions"/>.
        /// </summary>
        public HandleExceptionFilterOptions()
        {
            this.ResolveHandlerAction = action => $"On{action}Error";
            this.IncludeInnerException = false;
            this.ErrorViewName = "Error";
            this.JsonSerializerSettings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
        }
    }
}
