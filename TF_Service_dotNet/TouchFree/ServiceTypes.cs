using System;
using System.Numerics;

namespace Ultraleap.TouchFree.Library;

public readonly record struct InputAction(
    long Timestamp,
    InteractionType InteractionType,
    HandType HandType,
    HandChirality Chirality,
    InputType InputType,
    in Vector2 CursorPosition,
    float DistanceFromScreen,
    float ProgressToClick)
{
    public InputAction(long timestamp, InteractionType interactionType, HandType handType, HandChirality chirality,
        InputType inputType, in Positions positions, float progressToClick)
        : this(timestamp, interactionType, handType, chirality, inputType, positions.CursorPosition,
            positions.DistanceFromScreen, progressToClick)
    { }
}

public enum HandChirality
{
    LEFT,
    RIGHT
}

public enum HandType
{
    PRIMARY,
    SECONDARY,
}

public enum InputType
{
    NONE,
    CANCEL,
    DOWN,
    MOVE,
    UP,
}

public enum InteractionType
{
    GRAB,
    HOVER,
    PUSH,
    TOUCHPLANE,
    VELOCITYSWIPE,
    AIRCLICK,
}

[Flags]
public enum BitmaskFlags
{
    NONE = 0,

    // HandChirality
    LEFT = 1,
    RIGHT = 2,

    // Hand Type
    PRIMARY = 4,
    SECONDARY = 8,

    // Input Types
    NONE_INPUT = 16,
    CANCEL = 32,
    DOWN = 64,
    MOVE = 128,
    UP = 256,

    // Interaction Types
    GRAB = 512,
    HOVER = 1024,
    PUSH = 2048,
    TOUCHPLANE = 4096,
    VELOCITYSWIPE = 8192,

    // Adding elements to this list is a breaking change, and should cause at
    // least a minor iteration of the API version UNLESS adding them at the end
}

[Serializable]
public readonly record struct WebsocketInputAction(
    long Timestamp,
    BitmaskFlags InteractionFlags,
    in WebSocketVector2 CursorPosition,
    float DistanceFromScreen,
    float ProgressToClick)
{
    public static explicit operator WebsocketInputAction(in InputAction inputAction) => new(inputAction.Timestamp,
        Utilities.GetInteractionFlags(inputAction.InteractionType,
            inputAction.HandType,
            inputAction.Chirality,
            inputAction.InputType),
        inputAction.CursorPosition,
        inputAction.DistanceFromScreen,
        inputAction.ProgressToClick);
}

/// <param name="CursorPosition">Cursor position is used to guide the position of the cursor representation. It is calculated in Screen Space</param>
/// <param name="DistanceFromScreen"> Physical distance of the hand from the screen in meters.</param>
public readonly record struct Positions(in Vector2 CursorPosition, float DistanceFromScreen);

[Serializable]
public enum TrackedPosition
{
    INDEX_STABLE,
    INDEX_TIP,
    WRIST,
    NEAREST,
    HAND_POINTING,
    HAND_PROJECTION
}

// TODO: Remove this and just use System.Numerics.Vector2 when breaking protocol
[Serializable]
public readonly record struct WebSocketVector2(float x, float y)
{
    public static implicit operator WebSocketVector2(in Vector2 v2) => new(v2.X, v2.Y);
}