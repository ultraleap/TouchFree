using Leap;
using System;
using System.Collections.Generic;

namespace Ultraleap.TouchFree.Library
{
    public interface IHandManager
    {
        Hand PrimaryHand { get; }
        Hand SecondaryHand { get; }
        HandFrame RawHands { get; }
        List<Vector> RawHandPositions { get; }
        long Timestamp { get; }
        void ConnectToTracking();
        void DisconnectFromTracking();
        bool TrackingServiceConnected();
        bool CameraConnected();
        event Action HandFound;
        event Action HandsLost;
    }
}
