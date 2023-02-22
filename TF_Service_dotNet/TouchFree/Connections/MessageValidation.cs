using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Ultraleap.TouchFree.Library.Configuration;

namespace Ultraleap.TouchFree.Library.Connections
{
    public static class MessageValidation
    {
        private static readonly Dictionary<string, Type> configJsonPropertyTypes = new()
        {
            { "interaction", typeof(InteractionConfig) },
            { "physical", typeof(PhysicalConfig) }
        };

        /// <summary>
        /// Checks for invalid states of the config request
        /// </summary>
        /// <param name="contentObj">JObject of content</param>
        /// <returns>Returns a response as to the validity of the _content</returns>
        public static Result<Empty> ValidateConfigJson(JObject contentObj)
        {
            bool anyValidContentFound = false;
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
            }

            if (!anyValidContentFound)
            {
                return new Error("Setting configuration failed: No valid configuration content was found in the message");
            }

            return Result.Success;
        }

        /// <summary>
        /// Validates request id in the given message json object
        /// </summary>
        public static Result<string> ValidateRequestId(JObject content)
        {
            var idExists = content.TryGetValue("requestID", out var id);
            if (!idExists) return new Error("No requestID property found in json object");
            var requestId = id.ToString();
            return requestId != string.Empty // TODO: Regex validation? Do request IDs have a specific format?
                ? requestId
                : new Error("Request ID found in message is empty");
        }
    }
}