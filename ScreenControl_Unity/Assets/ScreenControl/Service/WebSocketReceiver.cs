using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Concurrent;
using Ultraleap.ScreenControl.Service.ScreenControlTypes;
using Ultraleap.ScreenControl.Core;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Ultraleap.ScreenControl.Service
{
    [DisallowMultipleComponent]
    public class WebSocketReceiver : MonoBehaviour
    {
        public ConcurrentQueue<string> setConfigQueue = new ConcurrentQueue<string>();
        public ConcurrentQueue<string> configRequestQueue = new ConcurrentQueue<string>();

        void Update()
        {
            CheckSetConfigQueue();
            CheckConfigRequestQueue();
        }

        void CheckConfigRequestQueue()
        {
            string content;
            if (configRequestQueue.TryPeek(out content))
            {
                // Parse newly received messages
                configRequestQueue.TryDequeue(out content);
                HandleConfigRequest(content);
            }
        }

        void HandleConfigRequest(string _content) 
        {
            JObject contentObj = JsonConvert.DeserializeObject<JObject>(_content);

            // Explicitly check for requestID because it is the only required key
            if (!contentObj.ContainsKey("requestID") || contentObj.GetValue("requestID").ToString() == "")
            {
                ResponseToClient response = new ResponseToClient("", "Failure", "", _content);
                response.message = "Configuration request failed. This is due to a missing or invalid requestID";

                // This is a failed request, do not continue with sendingthe configuration,
                // the Client will have no way to handle the config state
                WebSocketClientConnection.Instance.SendConfigurationResponse(response);
                return;
            }

            ScreenControlConfiguration currentConfig = new ScreenControlConfiguration(
                contentObj.GetValue("requestID").ToString(),
                ConfigManager.InteractionConfig,
                ConfigManager.PhysicalConfig);

            WebSocketClientConnection.Instance.SendCurrentConfigState(currentConfig);
        }

        #region SetConfigState

        void CheckSetConfigQueue()
        {
            string content;
            if (setConfigQueue.TryPeek(out content))
            {
                // Parse newly received messages
                setConfigQueue.TryDequeue(out content);
                HandleNewConfigState(content);
            }
        }

        void HandleNewConfigState(string _content)
        {
            ResponseToClient response = ValidateNewConfigState(_content);

            if(response.status == "Success")
            {
                SetConfigState(_content);
            }

            WebSocketClientConnection.Instance.SendConfigurationResponse(response);
        }

        /// <summary>
        /// Checks for invalid states of the config request
        /// </summary>
        /// <param name="_content">The whole json content of the request</param>
        /// <returns>Returns a response as to the validity of the _content</returns>
        ResponseToClient ValidateNewConfigState(string _content)
        {
            ResponseToClient response = new ResponseToClient("", "Success", "", _content);

            JObject contentObj = JsonConvert.DeserializeObject<JObject>(_content);

            // Explicitly check for requestID because it is the only required key
            if(!contentObj.ContainsKey("requestID") || contentObj.GetValue("requestID").ToString() == "")
            {
                // Validation has failed because there is no valid requestID
                response.status = "Failure";
                response.message = "Setting configuration failed. This is due to a missing or invalid requestID";
                return response;
            }

            var configRequestFields = typeof(ScreenControlConfiguration).GetFields();
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

        void SetConfigState(string _content)
        {
            ScreenControlConfiguration combinedData = new ScreenControlConfiguration("", ConfigManager.InteractionConfig, ConfigManager.PhysicalConfig);

            JsonUtility.FromJsonOverwrite(_content, combinedData);

            ConfigManager.InteractionConfig = combinedData.interaction;
            ConfigManager.PhysicalConfig = combinedData.physical;

            ConfigManager.PhysicalConfig.ConfigWasUpdated();
            ConfigManager.InteractionConfig.ConfigWasUpdated();
        }
        #endregion
    }
}