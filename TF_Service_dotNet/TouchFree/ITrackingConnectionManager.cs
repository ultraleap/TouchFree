namespace Ultraleap.TouchFree.Library
{
    public interface ITrackingConnectionManager
    {
        TrackingMode CurrentTrackingMode { get; }
        Leap.Controller controller { get; }
        void Connect();
        void Disconnect();
    }
}
