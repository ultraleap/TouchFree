using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Ultraleap.TouchFree.Library.Connections.MessageQueues
{
    public class TrackingApiChangeQueueHandler : MessageQueueHandler
    {
        public override ActionCode[] ActionCodes => new[] { ActionCode.GET_TRACKING_STATE, ActionCode.SET_TRACKING_STATE };

        protected override string noRequestIdFailureMessage => "Tracking State change request failed. This is due to a missing or invalid requestID";

        protected override ActionCode noRequestIdFailureActionCode => ActionCode.TRACKING_STATE;

        public TrackingResponse? trackingApiResponse = null;
        private float? responseOriginTime = null;
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

        protected override void CreateAndSendNoRequestIdError(IncomingRequest _request)
        {
            var maskResponse = new SuccessWrapper<MaskingData?>(false, noRequestIdFailureMessage, null);
            var boolResponse = new SuccessWrapper<bool?>(false, noRequestIdFailureMessage, null);

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
            clientMgr.SendResponse(state, noRequestIdFailureActionCode);
        }

        protected override void Handle(IncomingRequest _request, JObject _contentObject, string requestId)
        {
            _request.requestId = requestId;

            if (_request.action == ActionCode.GET_TRACKING_STATE)
            {
                HandleGetTrackingStateRequest(_request);
            }
            else
            {
                HandleSetTrackingStateRequest(_contentObject, _request);
            }
        }

        #region DiagnosticAPI_Requests

        void HandleGetTrackingStateRequest(IncomingRequest _request)
        {
            trackingApiResponse = new TrackingResponse(_request.requestId, _request.content, true, true, true, true, true);
            responseOriginTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

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
            responseOriginTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

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
                    responseOriginTime = null;

                    clientMgr.SendResponse(response.state, ActionCode.TRACKING_STATE);
                }
                else if (responseOriginTime.HasValue && DateTimeOffset.Now.ToUnixTimeMilliseconds() - responseOriginTime.Value > 30000f)
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
