using Newtonsoft.Json.Linq;
using System;
using Ultraleap.TouchFree.Library.Configuration;

namespace Ultraleap.TouchFree.Library.Connections.MessageQueues
{
    public class TrackingApiChangeQueueHandler : MessageQueueHandler
    {
        public override ActionCode[] HandledActionCodes => new[] { ActionCode.GET_TRACKING_STATE, ActionCode.SET_TRACKING_STATE };

        protected override string whatThisHandlerDoes => "Tracking State change request";

        protected override ActionCode failureActionCode => ActionCode.TRACKING_STATE;

        public TrackingResponse? trackingApiResponse = null;
        private float? responseOriginTime = null;
        private readonly IConfigManager configManager;
        private readonly ITrackingDiagnosticApi diagnosticApi;
        private readonly object trackingResponseLock = new object();

        public TrackingApiChangeQueueHandler(IUpdateBehaviour _updateBehaviour, IConfigManager _configManager, IClientConnectionManager _clientMgr, ITrackingDiagnosticApi _diagnosticApi) : base(_updateBehaviour, _clientMgr)
        {
            configManager = _configManager;
            diagnosticApi = _diagnosticApi;

            void ResetTrackingApiResponse() => trackingApiResponse = null;
            diagnosticApi.OnConnection += ResetTrackingApiResponse;
            diagnosticApi.OnDisconnection += ResetTrackingApiResponse;

            diagnosticApi.OnMaskingResponse += OnMasking;
            diagnosticApi.OnAllowImagesResponse += OnAllowImages;
            diagnosticApi.OnCameraOrientationResponse += OnCameraOrientation;
            diagnosticApi.OnAnalyticsResponse += OnAnalytics;
        }

        protected override void OnUpdate()
        {
            if (trackingApiResponse.HasValue)
            {
                CheckDApiResponse();
            }
            else
            {
                base.OnUpdate();
            }
        }

        protected override void HandleValidationError(IncomingRequestWithId request, Error error)
        {
            // TODO: Put error message somewhere in response
            var maskResponse = new SuccessWrapper<MaskingData?>(false, whatThisHandlerDoes, null);
            var boolResponse = new SuccessWrapper<bool?>(false, whatThisHandlerDoes, null);

            TrackingApiState state = new TrackingApiState()
            {
                requestID = "",
                mask = maskResponse,
                allowImages = boolResponse,
                cameraReversed = boolResponse,
                analyticsEnabled = boolResponse
            };

            // This is a failed request, do not continue with processing the request,
            // the Client will have no way to handle the config state
            clientMgr.SendResponse(state, failureActionCode);
        }

        protected override Result<Empty> ValidateContent(IncomingRequestWithId request)
        {
            if (request.ActionCode == ActionCode.SET_TRACKING_STATE)
            {
                var atLeastOneProperty = false;
                // TODO: Check types of properties are correct here too, then remove checks from HandleSetTrackingStateRequest below 
                atLeastOneProperty |= request.ContentRoot.ContainsKey("mask")
                                      || request.ContentRoot.ContainsKey("allowImages")
                                      || request.ContentRoot.ContainsKey("cameraReversed")
                                      || request.ContentRoot.ContainsKey("analyticsEnabled");
                if (!atLeastOneProperty) return new Error("Json contained no properties when attempting to Set Tracking State");
            }
            
            return Result.Success;
        }

        protected override void Handle(IncomingRequestWithId request)
        {
            if (request.ActionCode == ActionCode.GET_TRACKING_STATE)
            {
                HandleGetTrackingStateRequest(request);
            }
            else
            {
                HandleSetTrackingStateRequest(request);
            }
        }

        #region DiagnosticAPI_Requests

        private void HandleGetTrackingStateRequest(IncomingRequestWithId request)
        {
            trackingApiResponse = new TrackingResponse(request.RequestId, request.OriginalContent, true, true, true, true, true);
            responseOriginTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            diagnosticApi.RequestGetAllowImages();
            diagnosticApi.RequestGetImageMask();
            diagnosticApi.RequestGetCameraOrientation();
            diagnosticApi.RequestGetAnalyticsMode();
        }

        private void HandleSetTrackingStateRequest(IncomingRequestWithId request)
        {
            JToken maskToken;
            JToken allowImagesToken;
            JToken cameraReversedToken;
            JToken analyticsEnabledToken;

            var contentObj = request.ContentRoot;
            bool needsMask = contentObj.TryGetValue("mask", out maskToken);
            bool needsImages = contentObj.TryGetValue("allowImages", out allowImagesToken);
            bool needsOrientation = contentObj.TryGetValue("cameraReversed", out cameraReversedToken);
            bool needsAnalytics = contentObj.TryGetValue("analyticsEnabled", out analyticsEnabledToken);

            trackingApiResponse = new TrackingResponse(request.RequestId, request.OriginalContent, false, needsMask, needsImages, needsOrientation, needsAnalytics);
            responseOriginTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            var trackingFromFile = configManager.TrackingConfig;

            if (needsMask)
            {
                var mask = maskToken!.ToObject<MaskingData>();
                diagnosticApi.RequestSetImageMask(mask.left, mask.right, mask.upper, mask.lower);
                trackingFromFile.Mask.Left = mask.left;
                trackingFromFile.Mask.Right = mask.right;
                trackingFromFile.Mask.Upper = mask.upper;
                trackingFromFile.Mask.Lower = mask.lower;
            }

            if (needsImages)
            {
                var allowImages = allowImagesToken!.ToObject<bool>();
                diagnosticApi.RequestSetAllowImages(allowImages);
                trackingFromFile.AllowImages = allowImages;
            }

            if (needsOrientation)
            {
                var reversed = cameraReversedToken!.ToObject<bool>();
                diagnosticApi.RequestSetCameraOrientation(reversed);
                trackingFromFile.CameraReversed = reversed;
            }

            if (needsAnalytics)
            {
                var analyticsEnable = analyticsEnabledToken!.ToObject<bool>();
                diagnosticApi.RequestSetAnalyticsMode(analyticsEnable);
                trackingFromFile.AnalyticsEnabled = analyticsEnable;
            }

            if (needsAnalytics || needsImages || needsMask || needsOrientation)
            {
                TrackingConfigFile.SaveConfig(trackingFromFile);
            }
        }

        private void CheckDApiResponse()
        {
            lock (trackingResponseLock)
            {
                if (trackingApiResponse?.IsReady == true)
                {
                    TrackingResponse response = trackingApiResponse.Value;
                    trackingApiResponse = null;
                    responseOriginTime = null;

                    clientMgr.SendResponse(response.state, ActionCode.TRACKING_STATE);
                }
                // Timeout response if we've been waiting too long
                else if (responseOriginTime.HasValue && DateTimeOffset.Now.ToUnixTimeMilliseconds() - responseOriginTime.Value > 3000f || !diagnosticApi.IsConnected)
                {
                    TrackingResponse response = trackingApiResponse.Value;
                    trackingApiResponse = null;
                    responseOriginTime = null;

                    string message = "Tracking State change request failed; no response from Tracking within timeout period";

                    var maskResponse = new SuccessWrapper<MaskingData?>(false, message, null);
                    var boolResponse = new SuccessWrapper<bool?>(false, message, null);

                    if (response.needsMask)
                    {
                        response.state.mask = maskResponse;
                    }

                    if (response.needsImages)
                    {
                        response.state.allowImages = boolResponse;
                    }

                    if (response.needsOrientation)
                    {
                        response.state.cameraReversed = boolResponse;
                    }

                    if (response.needsAnalytics)
                    {
                        response.state.analyticsEnabled = boolResponse;
                    }

                    clientMgr.SendResponse(response.state, ActionCode.TRACKING_STATE);
                }
            }
        }

        private void OnMasking(Result<ImageMaskData> imageMask)
        {
            if (trackingApiResponse.HasValue)
            {
                var response = trackingApiResponse.Value;

                response.state.mask = imageMask.Match(mask =>
                {
                    var convertedMask = new MaskingData((float)mask.lower, (float)mask.upper, (float)mask.right,
                        (float)mask.left);
                    return new SuccessWrapper<MaskingData?>(true, "Image Mask State", convertedMask);
                }, error => new SuccessWrapper<MaskingData?>(false, error.Message, null));

                response.needsMask = false;
                trackingApiResponse = response;
            }
        }

        private void OnAllowImages(Result<bool> allowImages)
        {
            if (trackingApiResponse.HasValue)
            {
                var response = trackingApiResponse.Value;

                response.state.allowImages =
                    allowImages.Match(value => new SuccessWrapper<bool?>(true, "AllowImages State", value),
                        error => new SuccessWrapper<bool?>(false, error.Message, null));

                response.needsImages = false;
                trackingApiResponse = response;
            }
        }

        private void OnCameraOrientation(Result<bool> cameraReversed)
        {
            if (trackingApiResponse.HasValue)
            {
                var response = trackingApiResponse.Value;

                response.state.cameraReversed = cameraReversed.Match(
                    value => new SuccessWrapper<bool?>(true, "CameraOrientation State", value),
                    error => new SuccessWrapper<bool?>(false, error.Message, null));

                response.needsOrientation = false;
                trackingApiResponse = response;
            }
        }

        private void OnAnalytics(Result<bool> analytics)
        {
            if (trackingApiResponse.HasValue)
            {
                var response = trackingApiResponse.Value;

                response.state.analyticsEnabled = analytics.Match(
                    value => new SuccessWrapper<bool?>(true, "Analytics State", value),
                    error => new SuccessWrapper<bool?>(false, error.Message, null));

                response.needsAnalytics = false;
                trackingApiResponse = response;
            }
        }
        #endregion
    }
}
