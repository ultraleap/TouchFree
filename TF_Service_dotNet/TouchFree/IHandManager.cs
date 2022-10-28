using Leap;
using System;
using System.Collections.Generic;
using Ultraleap.TouchFree.Library.Connections;

namespace Ultraleap.TouchFree.Library
{
    public interface IHandManager
    {
        Hand PrimaryHand { get; }
        Hand SecondaryHand { get; }
        HandFrame RawHands { get; }
        List<Vector> RawHandPositions { get; }
        long Timestamp { get; }
        event Action HandFound;
        event Action HandsLost;
        Image.CameraType HandRenderLens { set; }
        ITrackingConnectionManager ConnectionManager { get; }
        ArraySegment<byte> LastImageData { get; }
    }
}
