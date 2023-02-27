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
        
        public static JsonSerializerSettings JsonSerializerSettings { get; } = new()
        {
            MissingMemberHandling = MissingMemberHandling.Error,
            NullValueHandling = NullValueHandling.Ignore
        };

        /// <summary>
        /// Checks for invalid states of the config request
        /// </summary>
        /// <param name="contentObj">JObject of content</param>
        /// <returns>Returns a response as to the validity of the _content</returns>
        public static Result<Empty> ValidateConfigJson(JObject contentObj)
        {
            var invalidProperties = new List<Error>();
            bool anyValidContentFound = false;
            foreach (var contentElement in contentObj)
            {
                if (configJsonPropertyTypes.TryGetValue(contentElement.Key, out Type T))
                {
                    try
                    {
                        // Check that _contentObj matches ConfigState structure, including all types
                        JsonConvert.DeserializeObject(contentElement.Value!.ToString(), T, JsonSerializerSettings);
                        anyValidContentFound = true;
                    }
                    catch (JsonException ex)
                    {
                        // Note: missing or unrecognized members will only be discovered 1 at a time in each type
                        invalidProperties.Add(new Error($"Configuration validation failed: {ex.Message}"));
                    }
                }
                else if (!validConfigContentKeys.Contains(contentElement.Key))
                {
                    invalidProperties.Add(new Error($"Unknown content key `{contentElement.Key}`"));
                }
            }

            return invalidProperties.Count switch
            {
                0 when anyValidContentFound => Result.Success,
                0 => new Error("Setting configuration failed: No valid configuration content was found in the message"),
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