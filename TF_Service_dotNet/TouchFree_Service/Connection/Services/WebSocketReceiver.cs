using System.Collections.Generic;
using System.Collections.Concurrent;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Ultraleap.TouchFree.Library.Configuration;
using Ultraleap.TouchFree.Service.ConnectionTypes;
using System;

namespace Ultraleap.TouchFree.Service.Connection
{
    public class WebSocketReceiver
    {
        public ConcurrentQueue<string> configChangeQueue = new ConcurrentQueue<string>();
        public ConcurrentQueue<string> configStateRequestQueue = new ConcurrentQueue<string>();
        public ConcurrentQueue<string> requestServiceStatusQueue = new ConcurrentQueue<string>();
        public ConcurrentQueue<string> configFileChangeQueue = new ConcurrentQueue<string>();
        public ConcurrentQueue<string> configFileRequestQueue = new ConcurrentQueue<string>();

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
            CheckQueue(configStateRequestQueue, HandleConfigStateRequest);
            CheckQueue(configChangeQueue, HandleConfigChange);

            CheckQueue(requestServiceStatusQueue, HandleGetStatusRequest);

            CheckQueue(configFileChangeQueue, HandleConfigFileRequest);
            CheckQueue(configFileRequestQueue, HandleConfigFileChange);
        }

        void CheckQueue(ConcurrentQueue<string> queue, Action<string> handler)
        {
            string content;
            if (queue.TryPeek(out content))
            {
                // Parse newly received messages
                queue.TryDequeue(out content);
                handler.Invoke(content);
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
                configManager.InteractionConfig.ForApi(),
                configManager.PhysicalConfig.ForApi());


            clientMgr.SendConfigState(currentConfig);
        }

        void HandleGetStatusRequest(string _content)
        {
            JObject contentObj = JsonConvert.DeserializeObject<JObject>(_content);

            // Explicitly check for requestID because it is the only required key
            if (!contentObj.ContainsKey("requestID") || contentObj.GetValue("requestID").ToString() == "")
            {
                ResponseToClient response = new ResponseToClient("", "Failure", "", _content);
                response.message = "Config state request failed. This is due to a missing or invalid requestID";

                // This is a failed request, do not continue with sendingthe status,
                // the Client will have no way to handle the config state
                clientMgr.SendStatusResponse(response);
                return;
            }

            TrackingServiceState trackingServiceState = TrackingServiceState.UNAVAILABLE;
            if (clientMgr.handManager.TrackingServiceConnected())
            {
                trackingServiceState = clientMgr.handManager.CameraConnected() ? TrackingServiceState.CONNECTED : TrackingServiceState.NO_CAMERA;
            }

            ServiceStatus currentConfig = new ServiceStatus(
                contentObj.GetValue("requestID").ToString(),
                trackingServiceState,
                configManager.ErrorLoadingConfigFiles ? ConfigurationState.ERRORED : ConfigurationState.LOADED);


            clientMgr.SendStatus(currentConfig);
        }

        void HandleConfigFileRequest(string _content)
        {
            // Return the current state of the config file(s) as JSON
        }

        #region Config Changes
        void HandleConfigChange(string _content)
        {
            ResponseToClient response = ValidateConfigChange(_content);

            if (response.status == "Success")
            {
                ChangeConfig(_content);
            }

            clientMgr.SendConfigChangeResponse(response);
        }

        void HandleConfigFileChange(string _content)
        {
            // Validate the incoming change
            ResponseToClient response = ValidateConfigChange(_content);

            if (response.status == "Success")
            {
                // Try saving config
                // If not work, return error
                // If work, send response from above
                try {
                    ChangeConfigFile(_content);
                }
                catch (UnauthorizedAccessException e)
                {
                    // Return some response indicating access authorisation issues
                    String errorMsg = "Did not have appropriate file access to modify the config file(s).";
                    response = new ResponseToClient(response.requestID, "Failed", errorMsg, _content);
                }
            }

            clientMgr.SendConfigFileChangeResponse(response);
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
            ConfigState combinedData = new ConfigState("", configManager.InteractionConfig.ForApi(), configManager.PhysicalConfig.ForApi());

            JsonConvert.PopulateObject(_content, combinedData);

            configManager.InteractionConfigFromApi = combinedData.interaction;
            configManager.PhysicalConfigFromApi = combinedData.physical;

            configManager.PhysicalConfigWasUpdated();
            configManager.InteractionConfigWasUpdated();
        }

        void ChangeConfigFile(string _content)
        {
            // Get the current state of the config file(s)
            InteractionConfig intFromFile = InteractionConfigFile.LoadConfig();
            InteractionConfigInternal interactions = new InteractionConfigInternal(intFromFile);

            PhysicalConfig physFromFile = PhysicalConfigFile.LoadConfig();
            PhysicalConfigInternal physical = new PhysicalConfigInternal(physFromFile);

            var contentJson = JObject.Parse(_content);

            string physicalChanges = contentJson["physical"].ToString();
            string interactionChanges = contentJson["interaction"].ToString();

            JsonConvert.PopulateObject(physicalChanges, physical);
            PhysicalConfig newPhysFile = new PhysicalConfig(physical);

            JsonConvert.PopulateObject(interactionChanges, interactions);
            InteractionConfig newIntFile = new InteractionConfig(interactions);

            PhysicalConfigFile.SaveConfig(newPhysFile);
            InteractionConfigFile.SaveConfig(newIntFile);

            // (if the above doesn't also force the current status to be the adjusted status, make that so)
        }
        #endregion
    }
}