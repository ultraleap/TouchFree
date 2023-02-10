const presets = ['Solid (Light)', 'Solid (Dark)', 'Outline (Light)', 'Outline (Dark)', 'Custom'] as const;
export type StyleDefaults = (typeof presets)[number];

export const cursorSections = ['Outer Fill', 'Center Fill', 'Center Border'] as const;
export type CursorSectionColors = {
    [Property in (typeof cursorSections)[number]]: string;
};

export const styleDefaults: { [Property in StyleDefaults]: CursorSectionColors | undefined } = {
    'Solid (Light)': {
        'Center Border': '#000000',
        'Center Fill': '#ffffff',
        'Outer Fill': '#ffffff',
    },
    'Solid (Dark)': {
        'Center Border': '#ffffff',
        'Center Fill': '#000000',
        'Outer Fill': '#000000',
    },
    'Outline (Light)': {
        'Center Border': '#ffffff',
        'Center Fill': '#ffffff00',
        'Outer Fill': '#ffffff',
    },
    'Outline (Dark)': {
        'Center Border': '#000000',
        'Center Fill': '#00000000',
        'Outer Fill': '#000000',
    },
    Custom: undefined,
};

export interface VisualsConfigColor {
    r: number;
    g: number;
    b: number;
    a: number;
}

export interface VisualsConfig {
    cursorEnabled: boolean;
    cursorSizeCm: number;
    cursorRingThickness: number;
    activeCursorPreset: number;
    primaryCustomColor: VisualsConfigColor;
    secondaryCustomColor: VisualsConfigColor;
    tertiaryCustomColor: VisualsConfigColor;
    ctiEnabled: boolean;
    ctiFilePath: string;
    ctiHideTrigger: number;
    ctiShowAfterTimer: number;
    StartupUIShown: boolean;
}

export const defaultVisualsConfig: VisualsConfig = {
    cursorEnabled: true,
    cursorSizeCm: 0.25,
    cursorRingThickness: 0.15,
    activeCursorPreset: 0,
    primaryCustomColor: {
        r: 1.0,
        g: 1.0,
        b: 1.0,
        a: 1.0,
    },
    secondaryCustomColor: {
        r: 1.0,
        g: 1.0,
        b: 1.0,
        a: 1.0,
    },
    tertiaryCustomColor: {
        r: 0.0,
        g: 0.0,
        b: 0.0,
        a: 1.0,
    },
    ctiEnabled: false,
    ctiFilePath:
        // eslint-disable-next-line max-len
        'C:/Program Files/Ultraleap/TouchFree/SettingsUI/TouchFreeSettingsUI_Data/StreamingAssets/CallToInteract/AirPush_Portrait.mp4',
    ctiHideTrigger: 1,
    ctiShowAfterTimer: 10.0,
    StartupUIShown: false,
};
