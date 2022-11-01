using Newtonsoft.Json.Linq;

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
            trackingConnectionManager.SetImagesState(_contentObject.GetValue("enabled").ToString() == true.ToString());
            var lens = _contentObject.GetValue("lens")?.ToString()?.ToLower() == Leap.Image.CameraType.LEFT.ToString().ToLower() ? Leap.Image.CameraType.LEFT : Leap.Image.CameraType.RIGHT;
            handManager.HandRenderLens = lens;

            ResponseToClient response = new ResponseToClient(requestId, "Success", string.Empty, _request.content);
            clientMgr.SendResponse(response, ActionCode.SET_HAND_DATA_STREAM_STATE_RESPONSE);
        }
    }
}
