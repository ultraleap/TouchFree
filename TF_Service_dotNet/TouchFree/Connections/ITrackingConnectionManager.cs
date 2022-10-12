namespace Ultraleap.TouchFree.Library.Connections
{
    public interface ITrackingConnectionManager
    {
        TrackingMode CurrentTrackingMode { get; }
        Leap.Controller controller { get; }
        void Connect();
        void Disconnect();
        void SetImagesState(bool enabled);
        bool ShouldSendHandData { get; set; }
    }
}
