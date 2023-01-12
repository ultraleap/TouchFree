import {
    HoverAndHoldInteractionSettings,
    InteractionConfigFull,
    TouchPlaneInteractionSettings,
    TrackedPosition,
} from 'TouchFree/src/Configuration/ConfigurationTypes';
import { InteractionType } from 'TouchFree/src/TouchFreeToolingTypes';

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
    UseSwipeInteraction: false,
    DeadzoneRadius: 0.003,

    InteractionZoneEnabled: false,
    InteractionMinDistanceCm: 0.0,
    InteractionMaxDistanceCm: 25.0,

    InteractionType: InteractionType.PUSH,

    // Interaction-specific settings
    HoverAndHold: DefaultHoverAndHoldInteractionSettings,
    TouchPlane: DefaultTouchPlaneInteractionSettings,
};
