using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.ExceptionHandling.Mvc
{
    /// <summary>
    /// 
    /// </summary>
    public class HandleExceptionFilterOptions
    {
        /// <summary>
        /// 
        /// </summary>
        public string ExceptionPolicy { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ErrorViewName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Func<string, string> ResolveHandlerAction { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IncludeInnerException { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public JsonSerializerSettings JsonSerializerSettings { get; set; }

        /// <summary>
        /// 
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
