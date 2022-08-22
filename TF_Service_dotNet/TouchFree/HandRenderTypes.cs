using System;
using System.Numerics;

namespace Ultraleap.TouchFree.Library
{
    [Serializable]
    public struct HandFrame
    {
        public RawHand[] Hands { get; set; }
    }

    [Serializable]
    public struct RawHand
    {
        public bool CurrentPrimary { get; set; }
        public RawFinger[] Fingers { get; set; }
        public float WristWidth { get; set; }
        public Vector3 WristPosition { get; set; }
    }

    [Serializable]
    public struct RawFinger
    {
        public RawBone[] Bones { get; set; }
        public FingerType Type { get; set; }
    }

    public enum FingerType
    {
        TYPE_THUMB = 0,
        TYPE_INDEX = 1,
        TYPE_MIDDLE = 2,
        TYPE_RING = 3,
        TYPE_PINKY = 4,
        TYPE_UNKNOWN = -1
    }

    [Serializable]
    public struct RawBone
    {
        public Vector3 NextJoint { get; set; }
        public Vector3 PrevJoint { get; set; }
    }
}