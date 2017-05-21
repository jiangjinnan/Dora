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

        public IDictionary<string, ExceptionPolicyElement> Build()
        {
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

            Dictionary<string, ExceptionPolicyElement> policies = new Dictionary<string, ExceptionPolicyElement>();
            foreach (JProperty it in settings.Properties().Where(it=>it.Name != "alias"))
            {
                policies.Add(it.Name, BuildPolicy((JObject)it.Value));
            }
            return policies;
        }
        private ExceptionPolicyElement BuildPolicy(JObject policy)
        {
            var policyElement = new ExceptionPolicyElement();
            policyElement.PreHandlers.AddRange(this.BuildExceptionHandlers((policy["pre"] as JArray) ?? new JArray()));
            policyElement.PostHandlers.AddRange(this.BuildExceptionHandlers((policy["post"] as JArray) ?? new JArray()));
            policyElement.PolicyEntries.AddRange(this.BuildPolicyEntries(policy));
            return policyElement;
        }
        private IEnumerable<PolicyEntryElement> BuildPolicyEntries(JObject policy)
        {
            List<PolicyEntryElement> list = new List<PolicyEntryElement>();
            foreach (var it in policy)
            {
                if (it.Key != "pre" && it.Key != "post")
                {
                    string exceptionTypeName = _alias.TryGetValue(it.Key, out string realTypename)
                        ? realTypename
                        : it.Key;
                    JObject policyEntry = (JObject)it.Value;
                    PostHandlingAction postHandlingAction = (PostHandlingAction)(Enum.Parse(typeof(PostHandlingAction), policyEntry["postHandlingAction"].ToString()));
                    PolicyEntryElement policyEntryElement = new PolicyEntryElement(Type.GetType(exceptionTypeName, true), postHandlingAction);
                    policyEntryElement.Handlers.AddRange(this.BuildExceptionHandlers((JArray)policyEntry["handlers"]));
                    list.Add(policyEntryElement);
                }
            }
            return list;
        }
        private IEnumerable<ExceptionHandlerElement> BuildExceptionHandlers(JArray handlers)
        {
            List<ExceptionHandlerElement> list = new List<ExceptionHandlerElement>();
            foreach (JObject handler in handlers)
            {
                string handlerTypeName = handler["type"].ToString();
                handlerTypeName = _alias.TryGetValue(handlerTypeName, out string realType)
                    ? realType
                    : handlerTypeName;
                ExceptionHandlerElement handlerElement = new ExceptionHandlerElement(Type.GetType(handlerTypeName, true));
                foreach (var it in handler)
                {
                    if (it.Key != "type")
                    {
                        handlerElement.Arguments.Add(new ArgumentElement(it.Key, it.Value.ToString()));
                    }
                }
                list.Add(handlerElement);
            }
            return list;
        }
    }
}
