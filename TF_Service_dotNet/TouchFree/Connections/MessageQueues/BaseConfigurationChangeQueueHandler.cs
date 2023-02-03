using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using Ultraleap.TouchFree.Library.Configuration;

namespace Ultraleap.TouchFree.Library.Connections.MessageQueues
{
    public abstract class BaseConfigurationChangeQueueHandler : MessageQueueHandler
    {
        protected BaseConfigurationChangeQueueHandler(IUpdateBehaviour _updateBehaviour, IClientConnectionManager _clientMgr) : base(_updateBehaviour, _clientMgr)
        {
        }

        protected override string noRequestIdFailureMessage => "Setting configuration failed. This is due to a missing or invalid requestID";

        protected Dictionary<string, Type> typeLookup;

        /// <summary>
        /// Checks for invalid states of the config request
        /// </summary>
        /// <param name="_content">The whole json content of the request</param>
        /// <returns>Returns a response as to the validity of the _content</returns>
        protected ResponseToClient ValidateConfigChange(string _content, JObject _contentObj)
        {
            ResponseToClient response = new ResponseToClient(string.Empty, "Success", string.Empty, _content);

            // Check for at least the request ID
            if (_contentObj.TryGetValue("requestID", out JToken id))
            {
                response.requestID = id.ToString();
            }
            else
            {
                response.status = "Failure";
                response.message = "A valid request ID was not found";
                return response;
            }

            if (typeLookup != null)
            {
                foreach (var contentElement in _contentObj)
                {
                    if (typeLookup.TryGetValue(contentElement.Key, out Type T))
                    {
                        try
                        {
                            // Check that _contentObj matches ConfigState structure, including all types
                            JsonConvert.DeserializeObject(contentElement.Value.ToString(), T);
                        }
                        catch (JsonReaderException ex)
                        {
                            response.status = "Failure";
                            response.message = "Setting configuration failed with message:\n\"" + ex.Message + "\"";
                            return response;
                        }
                    }
                }
            }

            return response;
        }
    }
}
