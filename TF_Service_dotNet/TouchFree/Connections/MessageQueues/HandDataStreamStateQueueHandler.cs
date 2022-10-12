using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Ultraleap.TouchFree.Library.Connections.MessageQueues
{
    public class HandDataStreamStateQueueHandler : MessageQueueHandler
    {
        private readonly ITrackingConnectionManager trackingConnectionManager;
        private readonly IHandManager handManager;

        public override ActionCode[] ActionCodes => new[] { ActionCode.SET_HAND_DATA_STREAM_STATE };

        protected override string noRequestIdFailureMessage => "Setting the hand data stream state failed. This is due to a missing or invalid requestID";

        protected override ActionCode noRequestIdFailureActionCode => ActionCode.SET_HAND_DATA_STREAM_STATE_RESPONSE;

        public HandDataStreamStateQueueHandler(IUpdateBehaviour _updateBehaviour, IClientConnectionManager _clientMgr, ITrackingConnectionManager _trackingConnectionManager, IHandManager _handManager) : base(_updateBehaviour, _clientMgr)
        {
            trackingConnectionManager = _trackingConnectionManager;
            handManager = _handManager;
        }

        protected override void Handle(IncomingRequest _request, JObject _contentObject, string requestId)
        {
            var enabled = _contentObject.GetValue("enabled").ToString() == true.ToString();

            HandDataReferenceFrame referenceFrame;
            if (!Enum.TryParse(_contentObject?.GetValue("referenceFrame")?.ToString(), out referenceFrame))
            {
                referenceFrame = HandDataReferenceFrame.LENS_FRAME;
            }

            handManager.HandDataReferenceFrame = referenceFrame;
            handManager.HandDataEnabled = enabled;
            trackingConnectionManager.ShouldSendHandData = enabled;

            switch (referenceFrame) {
                case HandDataReferenceFrame.SCREEN_FRAME:
                    trackingConnectionManager.SetImagesState(false);
                    break;
                default:
                    trackingConnectionManager.SetImagesState(enabled);
                    var lens = _contentObject.GetValue("lens")?.ToString()?.ToLower() == Leap.Image.CameraType.LEFT.ToString().ToLower() ? Leap.Image.CameraType.LEFT : Leap.Image.CameraType.RIGHT;
                    handManager.HandRenderLens = lens;
                    break;
            }

            ResponseToClient response = new ResponseToClient(requestId, "Success", string.Empty, _request.content);
            clientMgr.SendResponse(response, ActionCode.SET_HAND_DATA_STREAM_STATE_RESPONSE);
        }
    }
}
