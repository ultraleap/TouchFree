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
    public class WebSocketReceiver : MonoBehaviour
    {
        WebSocketClientConnection clientConnection;

        public ConcurrentQueue<string> setConfigQueue = new ConcurrentQueue<string>();

        public void SetWSClientConnection(WebSocketClientConnection _connection)
        {
            clientConnection = _connection;
        }

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
            ConfigResponse response = ValidateNewConfigState(_content);

            if(response.status == "Success")
            {
                SetConfigState(_content);
            }

            WebsocketClientConnection.Instance.SendConfigurationResponse(response);
        }

        /// <summary>
        /// Checks for invalid states of the config request
        /// </summary>
        /// <param name="_content">The whole json content of the request</param>
        /// <returns>Returns a response as to the validity of the _content</returns>
        ConfigResponse ValidateNewConfigState(string _content)
        {
            ConfigResponse response = new ConfigResponse("", "Success", "", _content);

            JObject contentObj = JsonConvert.DeserializeObject<JObject>(_content);

            // Explicitly check for requestID because it is the only required key
            if(!contentObj.ContainsKey("requestID") || contentObj.GetValue("requestID").ToString() == "")
            {
                // Validation has failed because there is no valid requestID
                response.status = "Failure";
                response.message = "Setting configuration failed. This is due to a missing or invalid requestID";
                return response;
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
                            response.status = "Failure";
                            response.message = "Setting configuration failed. This is due to an invalid field \"" + physicalContent.Key + "\"";
                            return response;
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