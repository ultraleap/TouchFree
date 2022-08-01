using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Ultraleap.TouchFree.Library.Connections.MessageQueues
{
    internal class TrackingApiChangeQueueHandler : MessageQueueHandler
    {
        public override ActionCode[] ActionCodes => new[] { ActionCode.GET_TRACKING_STATE, ActionCode.SET_TRACKING_STATE };

        public TrackingResponse? trackingApiResponse = null;
        private readonly ITrackingDiagnosticApi diagnosticApi;
        private readonly object trackingResponseLock = new object();

        public TrackingApiChangeQueueHandler(IUpdateBehaviour _updateBehaviour, IClientConnectionManager _clientMgr, ITrackingDiagnosticApi _diagnosticApi) : base(_updateBehaviour, _clientMgr)
        {
            diagnosticApi = _diagnosticApi;

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

        protected override void Handle(IncomingRequest _request)
        {
            JObject contentObj = JsonConvert.DeserializeObject<JObject>(_request.content);

            // Explicitly check for requestID else we can't respond
            if (!RequestIdExists(contentObj))
            {
                string message = "Tracking State change request failed. This is due to a missing or invalid requestID";

                var maskResponse = new SuccessWrapper<MaskingData?>(false, message, null);
                var boolResponse = new SuccessWrapper<bool?>(false, message, null);

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

        #region DiagnosticAPI_Requests

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

            trackingApiResponse = new TrackingResponse(_request.requestId, _request.content, false, needsMask, needsImages, needsOrientation, needsAnalytics);

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
            lock (trackingResponseLock)
            {
                if (trackingApiResponse.HasValue && ResponseIsReady(trackingApiResponse.Value))
                {
                    TrackingResponse response = trackingApiResponse.Value;
                    trackingApiResponse = null;

                    clientMgr.SendTrackingState(response.state);
                }
            }
        }

        public bool ResponseIsReady(TrackingResponse _response)
        {
            return (!_response.needsMask && !_response.needsImages && !_response.needsOrientation && !_response.needsAnalytics);
        }

        public void OnMasking(ImageMaskData? _mask, string _message)
        {
            if (trackingApiResponse.HasValue)
            {
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
    }
}
