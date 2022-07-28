using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Ultraleap.TouchFree.Library.Connections.MessageQueues
{
    public class HandDataStreamStateQueueHandler : MessageQueueHandler
    {
        private readonly ITrackingConnectionManager trackingConnectionManager;
        private readonly IHandManager handManager;

        public override ActionCode[] ActionCodes => new[] { ActionCode.SET_HAND_DATA_STREAM_STATE };

        public HandDataStreamStateQueueHandler(UpdateBehaviour _updateBehaviour, IClientConnectionManager _clientMgr, ITrackingConnectionManager _trackingConnectionManager, IHandManager _handManager) : base(_updateBehaviour, _clientMgr)
        {
            trackingConnectionManager = _trackingConnectionManager;
            handManager = _handManager;
        }

        protected override void Handle(IncomingRequest _request)
        {
            JObject contentObj = JsonConvert.DeserializeObject<JObject>(_request.content);

            // Explicitly check for requestID because it is the only required key
            if (!RequestIdExists(contentObj))
            {
                ResponseToClient failureResponse = new ResponseToClient(string.Empty, "Failure", string.Empty, _request.content);
                failureResponse.message = "Setting the hand data stream state failed. This is due to a missing or invalid requestID";

                // This is a failed request, do not continue with sending the status,
                // the Client will have no way to handle the config state
                clientMgr.SendHandDataStreamStateResponse(failureResponse);
                return;
            }


            trackingConnectionManager.SetImagesState(contentObj.GetValue("enabled").ToString() == true.ToString());
            var lens = contentObj.GetValue("lens")?.ToString()?.ToLower() == Leap.Image.CameraType.LEFT.ToString().ToLower() ? Leap.Image.CameraType.LEFT : Leap.Image.CameraType.RIGHT;
            handManager.HandRenderLens = lens;

            ResponseToClient response = new ResponseToClient(contentObj.GetValue("requestID").ToString(), "Success", string.Empty, _request.content);
            clientMgr.SendHandDataStreamStateResponse(response);
        }
    }
}
