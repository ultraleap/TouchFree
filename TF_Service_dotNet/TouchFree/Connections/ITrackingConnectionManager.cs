using System;

namespace Ultraleap.TouchFree.Library.Connections
{
    public interface ITrackingConnectionManager
    {
        TrackingMode CurrentTrackingMode { get; }
        Leap.IController Controller { get; }
        void Connect();
        void Disconnect();
        void SetImagesState(bool enabled);
        bool ShouldSendHandData { get; }
        TrackingServiceState TrackingServiceState { get; }
        event Action<TrackingServiceState> ServiceStatusChange;
    }
}
