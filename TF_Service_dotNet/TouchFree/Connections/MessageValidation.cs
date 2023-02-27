using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Ultraleap.TouchFree.Library.Configuration;

namespace Ultraleap.TouchFree.Library.Connections
{
    public static class MessageValidation
    {
        private static readonly string requestIdKeyName = nameof(ResponseToClient.requestID);
        private static readonly Dictionary<string, Type> configJsonPropertyTypes = new()
        {
            { "interaction", typeof(InteractionConfig) },
            { "physical", typeof(PhysicalConfig) }
        };

        private static readonly IReadOnlySet<string> validConfigContentKeys =
            configJsonPropertyTypes.Keys.Append(requestIdKeyName).ToHashSet();

        /// <summary>
        /// Checks for invalid states of the config request
        /// </summary>
        /// <param name="contentObj">JObject of content</param>
        /// <returns>Returns a response as to the validity of the _content</returns>
        public static Result<Empty> ValidateConfigJson(JObject contentObj)
        {
            bool anyValidContentFound = false;
            var invalidProperties = new List<Error>();
            foreach (var contentElement in contentObj)
            {
                if (configJsonPropertyTypes.TryGetValue(contentElement.Key, out Type T))
                {
                    try
                    {
                        // Check that _contentObj matches ConfigState structure, including all types
                        JsonConvert.DeserializeObject(contentElement.Value!.ToString(), T);
                        anyValidContentFound = true;
                    }
                    catch (JsonReaderException ex)
                    {
                        return new Error($"Setting configuration failed with message:\n\"{ex.Message}\"");
                    }
                }
                else if (!validConfigContentKeys.Contains(contentElement.Key))
                {
                    invalidProperties.Add(new Error($"Unknown content key `{contentElement.Key}`"));
                }
            }

            if (!anyValidContentFound)
            {
                return new Error("Setting configuration failed: No valid configuration content was found in the message");
            }

            return invalidProperties.Count switch
            {
                0 => Result.Success,
                1 => invalidProperties.Single(),
                _ => new Error("Multiple unknown keys in json", invalidProperties)
            };
        }

        /// <summary>
        /// Validates request id in the given message json object
        /// </summary>
        public static Result<string> ValidateRequestId(JObject content)
        {
            var idExists = content.TryGetValue(requestIdKeyName, out var id);
            if (!idExists) return new Error($"No {requestIdKeyName} property found in json object");
            var requestId = id.ToString();
            return requestId != string.Empty
                ? requestId
                : new Error($"{requestIdKeyName} found in message is empty");
        }
    }
}