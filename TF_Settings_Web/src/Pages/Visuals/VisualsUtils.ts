import { RgbaColor } from 'react-colorful';

const presets = ['Solid (Light)', 'Solid (Dark)', 'Outline (Light)', 'Outline (Dark)', 'Custom'] as const;
export type StyleDefaults = (typeof presets)[number];

export type CursorColors = [string, string, string];

export const styleDefaults: { [Property in StyleDefaults]: CursorColors | undefined } = {
    'Solid (Light)': ['#000000ff', '#ffffffff', '#ffffffff'],
    'Solid (Dark)': ['#ffffffff', '#000000ff', '#000000ff'],
    'Outline (Light)': ['#ffffffff', '#ffffff00', '#ffffffff'],
    'Outline (Dark)': ['#000000ff', '#00000000', '#000000ff'],
    Custom: undefined,
};

export interface VisualsConfig {
    cursorEnabled: boolean;
    cursorSizeCm: number;
    cursorRingThickness: number;
    activeCursorPreset: number;
    primaryCustomColor: RgbaColor;
    secondaryCustomColor: RgbaColor;
    tertiaryCustomColor: RgbaColor;
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
    primaryCustomColor: { r: 1, b: 1, g: 1, a: 1 },
    secondaryCustomColor: { r: 1, b: 1, g: 1, a: 1 },
    tertiaryCustomColor: { r: 0, b: 0, g: 0, a: 1 },
    ctiEnabled: false,
    ctiFilePath:
        // eslint-disable-next-line max-len
        'C:/Program Files/Ultraleap/TouchFree/SettingsUI/TouchFreeSettingsUI_Data/StreamingAssets/CallToInteract/AirPush_Portrait.mp4',
    ctiHideTrigger: 1,
    ctiShowAfterTimer: 10.0,
    StartupUIShown: false,
};
