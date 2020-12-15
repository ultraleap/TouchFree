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
    public class WebSocketReceiverHandler : MonoBehaviour
    {
        public ConcurrentQueue<string> setConfigQueue = new ConcurrentQueue<string>();

        void Update()
        {
            string content;
            if (setConfigQueue.TryPeek(out content))
            {
                // Parse newly received messages
                setConfigQueue.TryDequeue(out content);
                HandleNewConfigState(content);
            }
        }

        #region SetConfigState

        void HandleNewConfigState(string _content)
        {
            ConfigResponse response = new ConfigResponse("", "Success", "");

            if(ValidateNewConfigState(_content, ref response))
            {
                SetConfigState(_content);
            }

            WebsocketClientConnection.Instance.SendConfigurationResponse(response);
        }

        /// <summary>
        /// Checks for invalid states of the config request
        /// </summary>
        /// <param name="_content">The whole json content of the request</param>
        /// <param name="_response">A reference to be populated with appropriate data during validation</param>
        /// <returns>Returns the validity of the _content</returns>
        bool ValidateNewConfigState(string _content, ref ConfigResponse _response)
        {
            JObject contentObj = JsonConvert.DeserializeObject<JObject>(_content);

            // Explicitly check for requestID because it is the only required key
            if(!contentObj.ContainsKey("requestID") || contentObj.GetValue("requestID").ToString() == "")
            {
                // Validation has failed because there is no valid requestID
                _response.status = "Failed";
                _response.message = "Setting configuration failed. This is due to a missing or invalid requestID in the following content: \n\n" + _content;
                return false;
            }
            
            var configRequestFields = typeof(ConfigRequest).GetFields();
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
                    _response.status = "Failed";
                    _response.message = "Setting configuration failed. This is due to an invalid field \"" + contentElement.Key + "\" in the following content: \n\n" + _content;
                    return false;
                }

                if (contentElement.Key == "requestID")
                {
                    // We have a request ID so set it in the _response
                    _response.requestID = contentElement.Value.ToString();
                }

                if (contentElement.Key == "interaction")
                {
                    JObject interactionObj = JsonConvert.DeserializeObject<JObject>(contentElement.Value.ToString());

                    foreach (var interactionContent in interactionObj)
                    {
                        // this layer of _content should contain only fields that InteractionConfig owns
                        bool validInteractionField = IsFieldValid(interactionContent, interactionFields);

                        if (!validInteractionField)
                        {
                            // Validation has failed because the field is not valid
                            _response.status = "Failed";
                            _response.message = "Setting configuration failed. This is due to an invalid field \"" + interactionContent.Key + "\" in the following content: \n\n" + _content;
                            return false;
                        }

                        if (interactionContent.Key == "HoverAndHold")
                        {
                            JObject hoverAndHoldObj = JsonConvert.DeserializeObject<JObject>(interactionContent.Value.ToString());

                            foreach (var hoverAndHoldContent in hoverAndHoldObj)
                            {
                                // this layer of _content should contain only fields that HoverAndHoldInteractionSettings owns
                                bool validHoverField = IsFieldValid(hoverAndHoldContent, hoverAndHoldFields);

                                if (!validHoverField)
                                {
                                    // Validation has failed because the field is not valid
                                    _response.status = "Failed";
                                    _response.message = "Setting configuration failed. This is due to an invalid field \"" + hoverAndHoldContent.Key + "\" in the following content: \n\n" + _content;
                                    return false;
                                }
                            }
                        }
                    }
                }

                if (contentElement.Key == "physical")
                {
                    JObject physicalObj = JsonConvert.DeserializeObject<JObject>(contentElement.Value.ToString());

                    foreach (var physicalContent in physicalObj)
                    {
                        // this layer of _content should contain only fields that PhysicalConfig owns
                        bool validInteractionField = IsFieldValid(physicalContent, interactionFields);

                        if (!validInteractionField)
                        {
                            // Validation has failed because the field is not valid
                            _response.status = "Failed";
                            _response.message = "Setting configuration failed. This is due to an invalid field \"" + physicalContent.Key + "\" in the following content: \n\n" + _content;
                            return false;
                        }
                    }
                }
            }

            return true;
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
                        var converted = _field.Value.ToObject(configField.FieldType);
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
            ConfigRequest combinedData = new ConfigRequest("", ConfigManager.InteractionConfig, ConfigManager.PhysicalConfig);

            JsonUtility.FromJsonOverwrite(_content, combinedData);

            ConfigManager.InteractionConfig = combinedData.interaction;
            ConfigManager.PhysicalConfig = combinedData.physical;

            ConfigManager.PhysicalConfig.ConfigWasUpdated();
            ConfigManager.InteractionConfig.ConfigWasUpdated();
        }
        #endregion
    }
}