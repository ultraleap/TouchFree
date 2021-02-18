// Class: InteractionConfig
// This class is a container for all of the settings related to the interactions being processed
// by the ScreenControl Service. The settings at the root of this object will affect all
// sensations. There are also some settings specific to the Hover and Hold interaction which can
//  be modified by changing the contained <HoverAndHoldInteractionSettings>.
//
// In order to modify the settings of the service, create an instance of this class, make the
// changes you wish to see, and then send it to the server using the <ConfigurationManager>.
//
// Like all of the Settings classes found in this file, all members are optional. If you do
// not modify a member of this class, its value will not change when the instance is sent to
// ScreenControl Service.
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

    // Interaction-specific settings
    HoverAndHold: Partial<HoverAndHoldInteractionSettings>;
}

// Class: HoverAndHoldInteractionSettings
// This class is a container for settings that only apply to the Hover and Hold interaction. In
// order to modify these settings of the ScreenControl Service, create an <InteractionConfig>,
// which contains an instance of this class, modify it as required, and then pass to the service
// using the <ConfigurationManager>.
//
// Like all of the Settings classes found in this file, all members are optional. If you do
// not modify a member of this class, its value will not change when the instance is sent to
// ScreenControl Service.
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

// Class: PhysicalConfig
// This class is a container for all of the settings related to the physical setup of the
// hardware, both the tracking camera and the display.
//
// In order to modify the settings of the service, create an instance of this class, make the
// changes you wish to see, and then send it to the server using the <ConfigurationManager>.
//
// Like all of the Settings classes found in this file, all members are optional. If you do
// not modify a member of this class, its value will not change when the instance is sent to
// ScreenControl Service.
export interface PhysicalConfig {
    // Property: ScreenHeightM
    // The height of the screen in meters. This is needed in order to determine the relationship
    // between hand location in the real world and pixel locations on screen.
    ScreenHeightM: number;

    // Property: LeapPositionRelativeToScreenBottomM
    // The position (measured in meters) in 3d space of the Leap Motion camera relative to the
    // center of the bottom of the screen.
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