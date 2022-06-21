import {
    HoverAndHoldInteractionSettings,
    InteractionConfigFull,
    TouchPlaneInteractionSettings,
    TrackedPosition,
} from '../TouchFree/Configuration/ConfigurationTypes';
import { InteractionType } from '../TouchFree/TouchFreeToolingTypes';

const DefaultHoverAndHoldInteractionSettings: HoverAndHoldInteractionSettings = {
    HoverStartTimeS: 0.5,
    HoverCompleteTimeS: 0.6,
};

const DefaultTouchPlaneInteractionSettings: TouchPlaneInteractionSettings = {
    TouchPlaneActivationDistanceCm: 5,
    TouchPlaneTrackedPosition: TrackedPosition.NEAREST,
};

export const DefaultInteractionConfig: InteractionConfigFull = {
    UseScrollingOrDragging: false,
    DeadzoneRadius: 0.003,

    InteractionZoneEnabled: false,
    InteractionMinDistanceCm: 0.0,
    InteractionMaxDistanceCm: 25.0,

    InteractionType: InteractionType.PUSH,

    // Interaction-specific settings
    HoverAndHold: DefaultHoverAndHoldInteractionSettings,
    TouchPlane: DefaultTouchPlaneInteractionSettings,
};
