﻿using Leap;
using System.Linq;
using System.Numerics;

namespace Ultraleap.TouchFree.Library.Interactions.PositionTrackers
{
    public class IndexTipTracker : IPositionTracker
    {
        public TrackedPosition TrackedPosition => TrackedPosition.INDEX_TIP;

        public Vector3 GetTrackedPosition(Hand hand)
        {
            var tipPos = hand.Fingers.First(finger => (finger.Type == Finger.FingerType.TYPE_INDEX)).TipPosition;
            return Utilities.LeapVectorToNumerics(tipPos);
        }
    }
}
