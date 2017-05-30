using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dora.ExceptionHandling.Mvc
{
    public class HandleExceptionFilterOptions
    {
        public string ExceptionPolicy { get; set; }
        public string ErrorViewName { get; set; }
        public Func<string, string> ResolveHandlerAction { get; set; }
        public bool IncludeInnerException { get; set; }
        public JsonSerializerSettings JsonSerializerSettings { get; set; }
        public HandleExceptionFilterOptions()
        {
            this.ResolveHandlerAction = action => $"On{action}Error";
            this.IncludeInnerException = false;
            this.ErrorViewName = "Error";
            this.JsonSerializerSettings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
        }
    }
}
