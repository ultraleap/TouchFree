using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Ultraleap.TouchFree.Library;
using Ultraleap.TouchFree.Library.Connections;
using Ultraleap.TouchFree.Library.Configuration;
using Ultraleap.TouchFree.Library.Configuration.QuickSetup;
using Ultraleap.TouchFree.Service.ConnectionTypes;

#nullable enable
namespace Ultraleap.TouchFree.Service.Connection
{
    public class WebSocketReceiver
    {
        public ConcurrentQueue<string> configChangeQueue = new ConcurrentQueue<string>();
        public ConcurrentQueue<string> configStateRequestQueue = new ConcurrentQueue<string>();
        public ConcurrentQueue<string> configFileChangeQueue = new ConcurrentQueue<string>();
        public ConcurrentQueue<string> configFileRequestQueue = new ConcurrentQueue<string>();

        public ConcurrentQueue<string> requestServiceStatusQueue = new ConcurrentQueue<string>();
        public ConcurrentQueue<string> quickSetupQueue = new ConcurrentQueue<string>();

        public ConcurrentQueue<IncomingRequest> trackingApiChangeQueue = new ConcurrentQueue<IncomingRequest>();
        public TrackingResponse? trackingApiResponse = null;

        private readonly UpdateBehaviour updateBehaviour;
        private readonly ClientConnectionManager clientMgr;
        private readonly IConfigManager configManager;
        private readonly IQuickSetupHandler quickSetupHandler;
        private readonly ITrackingDiagnosticApi diagnosticApi;

        public WebSocketReceiver(UpdateBehaviour _updateBehaviour,
                                 ClientConnectionManager _clientMgr,
                                 IConfigManager _configManager,
                                 IQuickSetupHandler _quickSetupHandler,
                                 ITrackingDiagnosticApi _diagnosticApiManager)
        {
            clientMgr = _clientMgr;
            updateBehaviour = _updateBehaviour;
            configManager = _configManager;
            quickSetupHandler = _quickSetupHandler;
            diagnosticApi = _diagnosticApiManager;

            updateBehaviour.OnUpdate += Update;

            diagnosticApi.OnMaskingResponse += this.OnMasking;
            diagnosticApi.OnAllowImagesResponse += this.OnAllowImages;
            diagnosticApi.OnCameraOrientationResponse += this.OnCameraOrientation;
            diagnosticApi.OnAnalyticsResponse += this.OnAnalytics;
        }

        void Update()
        {
            CheckQueue(configChangeQueue, HandleConfigChange);
            CheckQueue(configStateRequestQueue, HandleConfigStateRequest);
            CheckQueue(configFileChangeQueue, HandleConfigFileChange);
            CheckQueue(configFileRequestQueue, HandleConfigFileRequest);

            CheckQueue(requestServiceStatusQueue, HandleGetStatusRequest);
            CheckQueue(quickSetupQueue, HandleQuickSetupRequest);

            if (trackingApiResponse.HasValue)
            {
                CheckDApiResponse();
            }
            else
            {
                CheckQueue(trackingApiChangeQueue, HandleTrackingRequest);
            }
        }

        static void CheckQueue<T>(ConcurrentQueue<T> _queue, Action<T> _handler)
        {
            T content;

            if (_queue.TryPeek(out content))
            {
                // Parse newly received messages
                _queue.TryDequeue(out content);
                _handler.Invoke(content);
            }
        }

        void HandleConfigStateRequest(string _content)
        {
            JObject contentObj = JsonConvert.DeserializeObject<JObject>(_content);

            // Explicitly check for requestID because it is the only required key
            if (!RequestIdExists(contentObj))
            {
                ResponseToClient response = new ResponseToClient(string.Empty, "Failure", string.Empty, _content);
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

        void HandleConfigFileRequest(string _content)
        {
            JObject contentObj = JsonConvert.DeserializeObject<JObject>(_content);

            // Explicitly check for requestID because it is the only required key
            if (!RequestIdExists(contentObj))
            {
                ResponseToClient response = new ResponseToClient(string.Empty, "Failure", string.Empty, _content);
                response.message = "Config state request failed. This is due to a missing or invalid requestID";

                // This is a failed request, do not continue with sending the configuration,
                // the Client will have no way to handle the config state
                clientMgr.SendConfigChangeResponse(response);
                return;
            }

            InteractionConfig interactions = InteractionConfigFile.LoadConfig();
            PhysicalConfig physical = PhysicalConfigFile.LoadConfig();

            ConfigState currentConfig = new ConfigState(
                contentObj.GetValue("requestID").ToString(),
                interactions,
                physical);

            clientMgr.SendConfigFile(currentConfig);
        }

        void HandleQuickSetupRequest(string _content)
        {
            QuickSetupRequest? quickSetupRequest = null;

            try
            {
                quickSetupRequest = JsonConvert.DeserializeObject<QuickSetupRequest>(_content);
            }
            catch { }

            // Explicitly check for requestID because it is the only required key
            if (string.IsNullOrWhiteSpace(quickSetupRequest?.requestID))
            {
                ResponseToClient response = new ResponseToClient(string.Empty, "Failure", string.Empty, _content);
                response.message = "Config state request failed. This is due to a missing or invalid requestID";

                // This is a failed request, do not continue with sending the configuration,
                // the Client will have no way to handle the config state
                clientMgr.SendConfigChangeResponse(response);
                return;
            }

            var quickSetupResponse = quickSetupHandler.HandlePositionRecording(quickSetupRequest.Value.Position);

            if (quickSetupResponse?.ConfigurationUpdated == true)
            {
                InteractionConfig interactions = InteractionConfigFile.LoadConfig();
                PhysicalConfig physical = PhysicalConfigFile.LoadConfig();

                ConfigState currentConfig = new ConfigState(
                    quickSetupRequest.Value.requestID,
                    interactions,
                    physical);

                clientMgr.SendQuickSetupConfigFile(currentConfig);
            }
            else if (quickSetupResponse?.PositionRecorded == true)
            {
                ResponseToClient response = new ResponseToClient(quickSetupRequest.Value.requestID, "Success", string.Empty, _content);
                clientMgr.SendQuickSetupResponse(response);
            }
            else
            {
                ResponseToClient response = new ResponseToClient(quickSetupRequest.Value.requestID, "Failure", quickSetupResponse?.QuickSetupError ?? string.Empty, _content);
                clientMgr.SendQuickSetupResponse(response);
            }
        }

        void HandleGetStatusRequest(string _content)
        {
            JObject contentObj = JsonConvert.DeserializeObject<JObject>(_content);

            // Explicitly check for requestID because it is the only required key
            if (!RequestIdExists(contentObj))
            {
                ResponseToClient response = new ResponseToClient(string.Empty, "Failure", string.Empty, _content);
                response.message = "Config state request failed. This is due to a missing or invalid requestID";

                // This is a failed request, do not continue with sending the status,
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

        #region DiagnosticAPI_Requests
        void HandleTrackingRequest(IncomingRequest _request)
        {
            JObject contentObj = JsonConvert.DeserializeObject<JObject>(_request.content);

            // Explicitly check for requestID else we can't respond
            if (!RequestIdExists(contentObj))
            {
                string message = "Tracking State change request failed. This is due to a missing or invalid requestID";

                var maskResponse = new SuccessWrapper<MaskingData?>(false, message, null);
                var boolResponse = new SuccessWrapper<bool?>(false, message, null);

                TrackingApiState state = new TrackingApiState() {
                    requestID =  "",
                    mask = maskResponse,
                    allowImages = boolResponse,
                    cameraReversed = boolResponse,
                    analyticsEnabled= boolResponse
                };

                // This is a failed request, do not continue with processing the request,
                // the Client will have no way to handle the config state
                clientMgr.SendTrackingState(state);
                return;
            }

            _request.requestId = contentObj.GetValue("requestID").ToString();

            if (_request.action == ActionCode.GET_TRACKING_STATE)
            {
                HandleGetTrackingStateRequest(_request);
            }
            else
            {
                HandleSetTrackingStateRequest(contentObj, _request);
            }
        }

        void HandleGetTrackingStateRequest(IncomingRequest _request)
        {
            trackingApiResponse = new TrackingResponse(_request.requestId, _request.content, true, true, true, true, true);

            diagnosticApi.GetAllowImages();
            diagnosticApi.GetImageMask();
            diagnosticApi.GetCameraOrientation();
            diagnosticApi.GetAnalyticsMode();
        }

        void HandleSetTrackingStateRequest(JObject contentObj, IncomingRequest _request)
        {
            JToken maskToken;
            JToken allowImagesToken;
            JToken cameraReversedToken;
            JToken analyticsEnabledToken;

            bool needsMask = contentObj.TryGetValue("mask", out maskToken);
            bool needsImages = contentObj.TryGetValue("allowImages", out allowImagesToken);
            bool needsOrientation = contentObj.TryGetValue("cameraReversed", out cameraReversedToken);
            bool needsAnalytics = contentObj.TryGetValue("analyticsEnabled", out analyticsEnabledToken);

            trackingApiResponse = new TrackingResponse(_request.requestId, _request.content, false, needsMask, needsImages, needsOrientation, needsMask);
 
            if (needsMask)
            {
                var mask = maskToken!.ToObject<MaskingData>();
                diagnosticApi.SetMasking(mask.left, mask.right, mask.upper, mask.lower);
            }

            if (needsImages)
            {
                var allowImages = allowImagesToken!.ToObject<bool>();
                diagnosticApi.SetAllowImages(allowImages);
            }

            if (needsOrientation)
            {
                var reversed = cameraReversedToken!.ToObject<bool>();
                diagnosticApi.SetCameraOrientation(reversed);
            }

            if (needsAnalytics)
            {
                var analyticsEnable = analyticsEnabledToken!.ToObject<bool>();
                diagnosticApi.SetAnalyticsMode(analyticsEnable);
            }
        }

        void CheckDApiResponse()
        {
            if (trackingApiResponse.HasValue && ResponseIsReady(trackingApiResponse.Value))
            {
                TrackingResponse response = trackingApiResponse.Value;
                trackingApiResponse = null;

                SendDApiResponse(response);
            }
        }

        void SendDApiResponse(TrackingResponse _response)
        {
            var content = JsonConvert.SerializeObject(_response.state);

            ActionCode action = _response.isGetRequest ? ActionCode.GET_TRACKING_STATE: ActionCode.SET_TRACKING_STATE;

            clientMgr.SendTrackingState(_response.state);
        }

        public bool ResponseIsReady(TrackingResponse _response)
        {
            return (!_response.needsMask && !_response.needsImages && !_response.needsOrientation && !_response.needsAnalytics);
        }

        public void OnMasking(ImageMaskData? _mask, string _message)
        {
            if (trackingApiResponse.HasValue) {
                var response = trackingApiResponse.Value;

                if (_mask.HasValue)
                {
                    var mask = _mask.Value;
                    var convertedMask = new MaskingData((float)mask.lower, (float)mask.upper, (float)mask.right, (float)mask.left);
                    response.state.mask = new SuccessWrapper<MaskingData?>(true, _message, convertedMask);
                }
                else
                {
                    response.state.mask = new SuccessWrapper<MaskingData?>(false, _message, null);
                }

                response.needsMask = false;
                trackingApiResponse = response;
            }
        }

        public void OnAllowImages(bool? _allowImages, string _message)
        {
            if (trackingApiResponse.HasValue)
            {
                var response = trackingApiResponse.Value;

                if (_allowImages.HasValue)
                {
                    response.state.allowImages = new SuccessWrapper<bool?>(true, _message, _allowImages.Value);
                }
                else
                {
                    response.state.allowImages = new SuccessWrapper<bool?>(false, _message, null);
                }

                response.needsImages = false;
                trackingApiResponse = response;
            }
        }

        public void OnCameraOrientation(bool? _cameraReversed, string _message)
        {
            if (trackingApiResponse.HasValue)
            {
                var response = trackingApiResponse.Value;

                if (_cameraReversed.HasValue)
                {
                    response.state.cameraReversed = new SuccessWrapper<bool?>(true, _message, _cameraReversed.Value);
                }
                else
                {
                    response.state.cameraReversed = new SuccessWrapper<bool?>(false, _message, null);
                }

                response.needsOrientation = false;
                trackingApiResponse = response;
            }
        }

        public void OnAnalytics(bool? _analytics, string _message)
        {
            if (trackingApiResponse.HasValue)
            {
                var response = trackingApiResponse.Value;


                if (_analytics.HasValue)
                {
                    response.state.analyticsEnabled = new SuccessWrapper<bool?>(true, _message, _analytics.Value);
                }
                else
                {
                    response.state.analyticsEnabled = new SuccessWrapper<bool?>(false, _message, null);
                }

                response.needsAnalytics = false;
                trackingApiResponse = response;
            }
        }
        #endregion

        private bool RequestIdExists(JObject _content)
        {
            if (!_content.ContainsKey("requestID") || _content.GetValue("requestID").ToString() == string.Empty)
            {
                return false;
            }

            return true;
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
                try
                {
                    ChangeConfigFile(_content);
                }
                catch (UnauthorizedAccessException _)
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
            ResponseToClient response = new ResponseToClient(string.Empty, "Success", string.Empty, _content);

            JObject contentObj = JsonConvert.DeserializeObject<JObject>(_content);

            // Explicitly check for requestID because it is the only required key
            if (!RequestIdExists(contentObj))
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
            ConfigState combinedData = new ConfigState(string.Empty, configManager.InteractionConfig.ForApi(), configManager.PhysicalConfig.ForApi());

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
            PhysicalConfig physFromFile = PhysicalConfigFile.LoadConfig();

            var contentJson = JObject.Parse(_content);

            string physicalChanges = contentJson["physical"].ToString();
            string interactionChanges = contentJson["interaction"].ToString();

            if (physicalChanges != string.Empty)
            {
                JsonConvert.PopulateObject(physicalChanges, physFromFile);
                PhysicalConfigFile.SaveConfig(physFromFile);
            }

            if (interactionChanges != string.Empty)
            {
                JsonConvert.PopulateObject(interactionChanges, intFromFile);
                InteractionConfigFile.SaveConfig(intFromFile);
            }
        }

        #endregion
    }
}