using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Ultraleap.TouchFree.Library.Connections.MessageQueues
{
    internal class TrackingApiChangeQueueHandler : MessageQueueHandler
    {
        public override ActionCode[] ActionCodes => new[] { ActionCode.GET_TRACKING_STATE, ActionCode.SET_TRACKING_STATE };

        public TrackingResponse? trackingApiResponse = null;
        private readonly ITrackingDiagnosticApi diagnosticApi;

        public TrackingApiChangeQueueHandler(IUpdateBehaviour _updateBehaviour, IClientConnectionManager _clientMgr, ITrackingDiagnosticApi _diagnosticApi) : base(_updateBehaviour, _clientMgr)
        {
            diagnosticApi = _diagnosticApi;

            diagnosticApi.OnMaskingResponse += OnMasking;
            diagnosticApi.OnAllowImagesResponse += OnAllowImages;
            diagnosticApi.OnCameraOrientationResponse += OnCameraOrientation;
            diagnosticApi.OnAnalyticsResponse += OnAnalytics;
        }

        protected override void Handle(IncomingRequest _request)
        {
            JObject contentObj = JsonConvert.DeserializeObject<JObject>(_request.content);

            // Explicitly check for requestID else we can't respond
            if (!RequestIdExists(contentObj))
            {
                ResponseToClient response = new ResponseToClient(string.Empty, "Failure", string.Empty, _request.content);
                response.message = "Tracking State change request failed. This is due to a missing or invalid requestID";

                // This is a failed request, do not continue with sending the status,
                // the Client will have no way to handle the config state
                clientMgr.SendTrackingResponse(response, _request.action);
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

            ActionCode action = _response.isGetRequest ? ActionCode.GET_TRACKING_STATE : ActionCode.SET_TRACKING_STATE;

            clientMgr.SendTrackingResponse(_response, action);
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

        private bool CheckSuccess<T>(SuccessWrapper<T>? wrapper)
        {
            if (wrapper.HasValue)
            {
                return wrapper.Value.succeeded;
            }
            else
            {
                return true;
            }
        }

        public bool ResponseIsReady(TrackingResponse _response)
        {
            return !_response.needsMask && !_response.needsImages && !_response.needsOrientation && !_response.needsAnalytics;
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
    }
}
