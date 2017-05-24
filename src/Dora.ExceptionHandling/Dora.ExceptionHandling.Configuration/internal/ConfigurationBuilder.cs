using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections;

namespace Dora.ExceptionHandling.Configuration
{
    internal class ConfigurationBuilder
    {
        private readonly IFileProvider _fileProvider;
        private readonly string _filePath;
        private readonly Dictionary<string, string> _alias;

        public ConfigurationBuilder(IFileProvider fileProvider, string filePath)
        {
            _fileProvider = fileProvider;
            _filePath = filePath;
            _alias = new Dictionary<string, string>();
        }

        public IDictionary<string, PolicyConfiguration> Build(out IDictionary<string, FilterConfiguration> exceptionFilters)
        {
            exceptionFilters = new Dictionary<string, FilterConfiguration>();
            JObject settings;
            using (var reader = new StreamReader(_fileProvider.GetFileInfo(_filePath).CreateReadStream()))
            {
                settings = JObject.Load(new JsonTextReader(reader));
            }
            var alias = settings["alias"];
            if (alias != null)
            {
                foreach (JProperty it in alias)
                {
                    _alias.Add(it.Name, it.Value.ToString());
                }
            }

            var filters = settings["filters"];
            if (filters != null)
            {
                foreach (JProperty it in alias)
                {
                    exceptionFilters.Add(it.Name, BuildeExceptionFilter((JObject)it.Value));
                }
            }

            Dictionary<string, PolicyConfiguration> policies = new Dictionary<string, PolicyConfiguration>();
            foreach (JProperty it in settings.Properties().Where(it=>it.Name != "alias" && it.Name != "filters"))
            {
                policies.Add(it.Name, BuildPolicy((JObject)it.Value));
            }
            return policies;
        }
        private FilterConfiguration BuildeExceptionFilter(JObject filter)
        {
            string filterTypeName = filter["type"].ToString();
            filterTypeName = _alias.TryGetValue(filterTypeName, out string realType)
                ? realType
                : filterTypeName;
            FilterConfiguration filterConfig = new FilterConfiguration(Type.GetType(filterTypeName, true));
            foreach (var it in filter)
            {
                if (it.Key != "type")
                {
                    filterConfig.Arguments.Add(new ArgumentConfiguration(it.Key, it.Value.ToString()));
                }
            }
            return filterConfig;
        }
        private PolicyConfiguration BuildPolicy(JObject policy)
        {
            var policyElement = new PolicyConfiguration();
            policyElement.PreHandlers.AddRange(this.BuildExceptionHandlers((policy["pre"] as JArray) ?? new JArray()));
            policyElement.PostHandlers.AddRange(this.BuildExceptionHandlers((policy["post"] as JArray) ?? new JArray()));
            policyElement.PolicyEntries.AddRange(this.BuildPolicyEntries(policy));
            return policyElement;
        }
        private IEnumerable<PolicyEntryConfiguration> BuildPolicyEntries(JObject policy)
        {
            List<PolicyEntryConfiguration> list = new List<PolicyEntryConfiguration>();
            foreach (var it in policy)
            {
                if (it.Key != "pre" && it.Key != "post")
                {
                    string exceptionTypeName = _alias.TryGetValue(it.Key, out string realTypename)
                        ? realTypename
                        : it.Key;
                    JObject policyEntry = (JObject)it.Value;
                    PostHandlingAction postHandlingAction = (PostHandlingAction)(Enum.Parse(typeof(PostHandlingAction), policyEntry["postHandlingAction"].ToString()));
                    PolicyEntryConfiguration policyEntryElement = new PolicyEntryConfiguration(Type.GetType(exceptionTypeName, true), postHandlingAction);
                    policyEntryElement.Handlers.AddRange(this.BuildExceptionHandlers((JArray)policyEntry["handlers"]));
                    list.Add(policyEntryElement);
                }
            }
            return list;
        }
        private IEnumerable<HandlerConfiguration> BuildExceptionHandlers(JArray handlers)
        {
            List<HandlerConfiguration> list = new List<HandlerConfiguration>();
            foreach (JObject handler in handlers)
            {
                string handlerTypeName = handler["type"].ToString();
                handlerTypeName = _alias.TryGetValue(handlerTypeName, out string realType)
                    ? realType
                    : handlerTypeName;
                HandlerConfiguration handlerConfig;
                string filter = handler["filter"]?.ToString();
                if (string.IsNullOrEmpty(filter))
                {
                    handlerConfig = new HandlerConfiguration(Type.GetType(handlerTypeName, true));
                }
                else
                {
                    handlerConfig = new FilterableHandlerConfiguration(Type.GetType(handlerTypeName, true), filter);
                }                
                foreach (var it in handler)
                {
                    if (it.Key != "type")
                    {
                        handlerConfig.Arguments.Add(new ArgumentConfiguration(it.Key, it.Value.ToString()));
                    }
                }
                list.Add(handlerConfig);
            }
            return list;
        }
    }
}
