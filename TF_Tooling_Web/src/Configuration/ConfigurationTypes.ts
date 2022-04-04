import { InteractionType } from "../TouchFreeToolingTypes";

// Class: InteractionConfig
// This class is a container for all of the settings related to the interactions being processed
// by the TouchFree Service. The settings at the root of this object will affect all
// sensations. There are also some settings specific to the Hover and Hold interaction which can
// be modified by changing the contained <HoverAndHoldInteractionSettings>.
//
// In order to modify the settings of the service, create an instance of this class, make the
// changes you wish to see, and then send it to the server using the <ConfigurationManager>.
//
// Like all of the Settings classes found in this file, all members are optional. If you do
// not modify a member of this class, its value will not change when the instance is sent to
// TouchFree Service.
export interface InteractionConfig {
    // Property: UseScrollingOrDragging
    // If true, allows interactions to send up/down events seperately, enabling dragging or
    // touchscreen-like scrolling behaviours. If false, up/down events will be sent together,
    // and every down will function like a click of its own.
    UseScrollingOrDragging: boolean;

    // Property: DeadzoneRadius
    // All interactions use a small deadzone to stabilise the position of the cursor, to prevent
    // small user movements from making the cursor shake in place. This setting controls the
    // radius of that deadzone.
    DeadzoneRadius: number;

    // Property: InteractionZoneEnabled
    // Changes whether the Interaction Zone values will be used
    InteractionZoneEnabled: boolean;

    // Property: InteractionMinDistanceCm
    // The minimum distance from the screen that users can interact within
    InteractionMinDistanceCm: number;

    // Property: InteractionMaxDistanceCm
    // The maximum distance from the screen that users can interact within
    InteractionMaxDistanceCm: number;

    // Property: InteractionType
    // This represents the type of interaction currently selected
    InteractionType: InteractionType;

    // Interaction-specific settings
    HoverAndHold: Partial<HoverAndHoldInteractionSettings>;
    TouchPlane: Partial<TouchPlaneInteractionSettings>;
}

// Class: InteractionConfig
// This class is duplicate of <InteractionConfigPartial> without the Interactions data being optional
// This form of <InteractionConfigPartial> is used in the <ConfigState> object returned when requsting
// the current state of the Service's config or its config files.
export interface InteractionConfigFull {
    UseScrollingOrDragging: boolean;

    DeadzoneRadius: number;

    InteractionZoneEnabled: boolean;

    InteractionMinDistanceCm: number;

    InteractionMaxDistanceCm: number;

    InteractionType: InteractionType;

    HoverAndHold: HoverAndHoldInteractionSettings;
    TouchPlane: TouchPlaneInteractionSettings;
}

// Class: HoverAndHoldInteractionSettings
// This class is a container for settings that only apply to the Hover and Hold interaction. In
// order to modify these settings of the TouchFree Service, create an <InteractionConfig>,
// which contains an instance of this class, modify it as required, and then pass to the service
// using the <ConfigurationManager>.
//
// Like all of the Settings classes found in this file, all members are optional. If you do
// not modify a member of this class, its value will not change when the instance is sent to
// TouchFree Service.
export interface HoverAndHoldInteractionSettings {
    // Property: HoverStartTimeS
    // This determines how long (in seconds) the user must hold their hand in place before the
    // interaction will begin. If the hand remains in place until the interaction completes,
    // a click event will be sent.
    HoverStartTimeS: number;

    // Property: HoverCompleteTimeS
    // This determines how long (in seconds) the user must hold their hand in place after the
    // interaction has begun before the interaction will complete, and a click event will be
    // sent.
    HoverCompleteTimeS: number;
}

// Class: TouchPlaneInteractionSettings
// This class is a container for settings that only apply to the TouchPlane interaction. In
// order to modify these settings of the TouchFree Service, create an <InteractionConfig>,
// which contains an instance of this class, modify it as required, and then pass to the service
// using the <ConfigurationManager>.
//
// Like all of the Settings classes found in this file, all members are optional. If you do
// not modify a member of this class, its value will not change when the instance is sent to
// TouchFree Service.
export interface TouchPlaneInteractionSettings {
    // Property: touchPlaneActivationDistanceCm
    // This determines how far (in cm) the TouchPlane is from the screen surface. This
    // represents the plane that the user must pass to begin and end a click event.
    touchPlaneActivationDistanceCm: number;

    // Property: TouchPlaneTrackedPosition
    // This determines which bone position will be tracked when performing the interaction.
    TouchPlaneTrackedPosition: TrackedPosition;
}

// Class: PhysicalConfig
// This class is a container for all of the settings related to the physical setup of the
// hardware, both the tracking camera and the display.
//
// In order to modify the settings of the service, create an instance of this class, make the
// changes you wish to see, and then send it to the server using the <ConfigurationManager>.
//
// Like all of the Settings classes found in this file, all members are optional. If you do
// not modify a member of this class, its value will not change when the instance is sent to
// TouchFree Service.
export interface PhysicalConfig {
    // Property: ScreenHeightM
    // The height of the screen in meters. This is needed in order to determine the relationship
    // between hand location in the real world and pixel locations on screen.
    ScreenHeightM: number;

    // Property: LeapPositionRelativeToScreenBottomM
    // The position (measured in meters) in 3d space of the Leap Motion camera relative to the
    // center of the bottom edge of the screen.
    //
    // This uses a left handed coordinate system where:
    // X = left/right (right = positive)
    // Y = up/down (up = positive)
    // Z = forward/backward (forward = positive)
    LeapPositionRelativeToScreenBottomM: Array<number>;

    // Property: LeapRotationD
    // The rotation of the Leap Motion Camera relative to the unity world space, measured in
    // degrees
    LeapRotationD: Array<number>;

    // Property: ScreenRotationD
    // The rotation of the physical screen relative to the unity world space, measured in
    // degrees
    ScreenRotationD: number;
}

// Enum: TrackedPosition
// INDEX_STABLE - Towards the screen from the proximal knuckle position of the index finger
// INDEX_TIP - The index finger tip position
// WRIST - The wrist position
// NEAREST - The nearest bone to the screen
export enum TrackedPosition {
    INDEX_STABLE,
    INDEX_TIP,
    WRIST,
    NEAREST
}