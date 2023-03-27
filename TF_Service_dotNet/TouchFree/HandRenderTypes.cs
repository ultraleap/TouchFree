using System;
using System.Numerics;

namespace Ultraleap.TouchFree.Library;

[Serializable] public readonly record struct HandFrame(in RawHand[] Hands);
[Serializable] public readonly record struct RawHand(in bool CurrentPrimary, in RawFinger[] Fingers, in Vector3 WristPosition, in float WristWidth);
[Serializable] public readonly record struct RawFinger(in FingerType Type, in RawBone[] Bones);
[Serializable] public readonly record struct RawBone(in Vector3 NextJoint, in Vector3 PrevJoint);

public enum FingerType
{
    TYPE_THUMB = 0,
    TYPE_INDEX = 1,
    TYPE_MIDDLE = 2,
    TYPE_RING = 3,
    TYPE_PINKY = 4,
    TYPE_UNKNOWN = -1
}