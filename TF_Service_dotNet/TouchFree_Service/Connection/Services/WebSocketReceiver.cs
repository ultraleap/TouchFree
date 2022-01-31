using System.Collections.Generic;
using System.Collections.Concurrent;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Ultraleap.TouchFree.Library.Configuration;
using Ultraleap.TouchFree.Service.ConnectionTypes;

namespace Ultraleap.TouchFree.Service.Connection
{
    public class WebSocketReceiver
    {
        public ConcurrentQueue<string> configChangeQueue = new ConcurrentQueue<string>();
        public ConcurrentQueue<string> configStateRequestQueue = new ConcurrentQueue<string>();

        private readonly UpdateBehaviour updateBehaviour;
        private readonly ClientConnectionManager clientMgr;
        private readonly IConfigManager configManager;

        public WebSocketReceiver(UpdateBehaviour _updateBehaviour, ClientConnectionManager _clientMgr, IConfigManager _configManager)
        {
            clientMgr = _clientMgr;
            updateBehaviour = _updateBehaviour;
            configManager = _configManager;

            updateBehaviour.OnUpdate += Update;
        }

        void Update()
        {
            CheckConfigChangeQueue();
            CheckConfigStateRequestQueue();
        }

        #region Config State Request

        void CheckConfigStateRequestQueue()
        {
            string content;
            if (configStateRequestQueue.TryPeek(out content))
            {
                // Parse newly received messages
                configStateRequestQueue.TryDequeue(out content);
                HandleConfigStateRequest(content);
            }
        }

        void HandleConfigStateRequest(string _content)
        {
            JObject contentObj = JsonConvert.DeserializeObject<JObject>(_content);

            // Explicitly check for requestID because it is the only required key
            if (!contentObj.ContainsKey("requestID") || contentObj.GetValue("requestID").ToString() == "")
            {
                ResponseToClient response = new ResponseToClient("", "Failure", "", _content);
                response.message = "Config state request failed. This is due to a missing or invalid requestID";

                // This is a failed request, do not continue with sendingthe configuration,
                // the Client will have no way to handle the config state
                clientMgr.SendConfigChangeResponse(response);
                return;
            }

            ConfigState currentConfig = new ConfigState(
                contentObj.GetValue("requestID").ToString(),
                configManager.InteractionConfig,
                configManager.PhysicalConfig);


            clientMgr.SendConfigState(currentConfig);
        }
        #endregion

        #region Config Change

        void CheckConfigChangeQueue()
        {
            string content;
            if (configChangeQueue.TryPeek(out content))
            {
                // Parse newly received messages
                configChangeQueue.TryDequeue(out content);
                HandleConfigChange(content);
            }
        }

        void HandleConfigChange(string _content)
        {
            ResponseToClient response = ValidateConfigChange(_content);

            if (response.status == "Success")
            {
                ChangeConfig(_content);
            }

            clientMgr.SendConfigChangeResponse(response);
        }

        /// <summary>
        /// Checks for invalid states of the config request
        /// </summary>
        /// <param name="_content">The whole json content of the request</param>
        /// <returns>Returns a response as to the validity of the _content</returns>
        ResponseToClient ValidateConfigChange(string _content)
        {
            ResponseToClient response = new ResponseToClient("", "Success", "", _content);

            JObject contentObj = JsonConvert.DeserializeObject<JObject>(_content);

            // Explicitly check for requestID because it is the only required key
            if (!contentObj.ContainsKey("requestID") || contentObj.GetValue("requestID").ToString() == "")
            {
                // Validation has failed because there is no valid requestID
                response.status = "Failure";
                response.message = "Setting configuration failed. This is due to a missing or invalid requestID";
                return response;
            }

            var configRequestFields = typeof(ConfigState).GetFields();
            var interactionFields = typeof(InteractionConfig).GetFields();
            var hoverAndHoldFields = typeof(HoverAndHoldInteractionSettings).GetFields();
            var physicalFields = typeof(PhysicalConfig).GetFields();

            foreach (var contentElement in contentObj)
            {
                // first layer of _content should contain only fields that ConfigRequest owns
                bool validField = IsFieldValid(contentElement, configRequestFields);

                if (!validField)
                {
                    // Validation has failed because the field is not valid
                    response.status = "Failure";
                    response.message = "Setting configuration failed. This is due to an invalid field \"" + contentElement.Key + "\"";
                    return response;
                }

                if (contentElement.Key == "requestID")
                {
                    // We have a request ID so set it in the _response
                    response.requestID = contentElement.Value.ToString();
                }

                if (contentElement.Key == "interaction")
                {
                    JObject interactionObj = JsonConvert.DeserializeObject<JObject>(contentElement.Value.ToString());

                    if (interactionObj != null)
                    {
                        foreach (var interactionContent in interactionObj)
                        {
                            // this layer of _content should contain only fields that InteractionConfig owns
                            bool validInteractionField = IsFieldValid(interactionContent, interactionFields);

                            if (!validInteractionField)
                            {
                                // Validation has failed because the field is not valid
                                response.status = "Failure";
                                response.message = "Setting configuration failed. This is due to an invalid field \"" + interactionContent.Key + "\"";
                                return response;
                            }

                            if (interactionContent.Key == "HoverAndHold")
                            {
                                JObject hoverAndHoldObj = JsonConvert.DeserializeObject<JObject>(interactionContent.Value.ToString());

                                if (hoverAndHoldObj != null)
                                {
                                    foreach (var hoverAndHoldContent in hoverAndHoldObj)
                                    {
                                        // this layer of _content should contain only fields that HoverAndHoldInteractionSettings owns
                                        bool validHoverField = IsFieldValid(hoverAndHoldContent, hoverAndHoldFields);

                                        if (!validHoverField)
                                        {
                                            // Validation has failed because the field is not valid
                                            response.status = "Failure";
                                            response.message = "Setting configuration failed. This is due to an invalid field \"" + hoverAndHoldContent.Key + "\"";
                                            return response;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (contentElement.Key == "physical")
                {
                    JObject physicalObj = JsonConvert.DeserializeObject<JObject>(contentElement.Value.ToString());

                    if (physicalObj != null)
                    {
                        foreach (var physicalContent in physicalObj)
                        {
                            // this layer of _content should contain only fields that PhysicalConfig owns
                            bool validPhysicslField = IsFieldValid(physicalContent, physicalFields);

                            if (!validPhysicslField)
                            {
                                // Validation has failed because the field is not valid
                                response.status = "Failure";
                                response.message = "Setting configuration failed. This is due to an invalid field \"" + physicalContent.Key + "\"";
                                return response;
                            }
                        }
                    }
                }
            }

            return response;
        }

        bool IsFieldValid(KeyValuePair<string, JToken> _field, System.Reflection.FieldInfo[] _possibleFields)
        {
            foreach (var configField in _possibleFields)
            {
                if (_field.Key == configField.Name)
                {
                    try
                    {
                        // Try to parse the value to the expected type, if it in invalid, we will catch th error and return false
                        _field.Value.ToObject(configField.FieldType);
                    }
                    catch
                    {
                        return false;
                    }

                    return true;
                }
            }
            return false;
        }

        void ChangeConfig(string _content)
        {
            ConfigState combinedData = new ConfigState("", configManager.InteractionConfig, configManager.PhysicalConfig);

            JsonConvert.PopulateObject(_content, combinedData);

            configManager.InteractionConfig = combinedData.interaction;
            configManager.PhysicalConfig = combinedData.physical;

            configManager.PhysicalConfigWasUpdated();
            configManager.InteractionConfigWasUpdated();
        }
        #endregion
    }
}