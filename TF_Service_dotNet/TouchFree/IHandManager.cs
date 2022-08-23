using System;
using System.Collections.Generic;
using Leap;

namespace Ultraleap.TouchFree.Library
{
    public delegate void ConnectionStatusEvent(bool oldIsConnected, bool newIsConnected);
    public interface IHandManager
    {
        Hand PrimaryHand { get; }
        Hand SecondaryHand { get; }
        HandFrame RawHands { get; }
        List<Vector> RawHandPositions { get; }
        long Timestamp { get; }
        void ConnectToTracking();
        void DisconnectFromTracking();
        bool IsTrackingServiceConnected();
        bool IsCameraConnected();
        event Action HandFound;
        event Action HandsLost;
        Leap.Image.CameraType HandRenderLens { set; }
        event ConnectionStatusEvent Camera;
    }
}
