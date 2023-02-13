import { ColorFormats } from 'tinycolor2';

export const presets = ['Solid (Light)', 'Solid (Dark)', 'Custom', 'Outline (Light)', 'Outline (Dark)'] as const;

export type CursorPreset = (typeof presets)[number];

export type CursorStyle = [string, string, string];

export const cursorStyles: { [Property in CursorPreset]: CursorStyle } = {
    'Solid (Light)': ['#ffffffff', '#ffffffff', '#000000ff'],
    'Solid (Dark)': ['#000000ff', '#000000ff', '#ffffffff'],
    'Outline (Light)': ['#ffffffff', '#ffffffff', '#ffffff00'],
    'Outline (Dark)': ['#000000ff', '#000000ff', '#00000000'],
    Custom: ['#ffffffff', '#ffffffff', '#000000ff'],
};

interface CursorVisualsConfig {
    cursorEnabled: boolean;
    cursorSizeCm: number;
    cursorRingThickness: number;
    activeCursorPreset: number;
    primaryCustomColor: ColorFormats.RGBA;
    secondaryCustomColor: ColorFormats.RGBA;
    tertiaryCustomColor: ColorFormats.RGBA;
}

interface CtiVisualsConfig {
    ctiEnabled: boolean;
    ctiFilePath: string;
    ctiHideTrigger: number;
    ctiShowAfterTimer: number;
    StartupUIShown: boolean;
}

export interface VisualsConfig extends CursorVisualsConfig, CtiVisualsConfig {}

export const defaultCursorVisualsConfig: CursorVisualsConfig = {
    cursorEnabled: true,
    cursorSizeCm: 0.25,
    cursorRingThickness: 0.15,
    activeCursorPreset: 0,
    primaryCustomColor: { r: 1, b: 1, g: 1, a: 1 },
    secondaryCustomColor: { r: 1, b: 1, g: 1, a: 1 },
    tertiaryCustomColor: { r: 0, b: 0, g: 0, a: 1 },
};

export const defaultCtiVisualsConfig: CtiVisualsConfig = {
    ctiEnabled: false,
    ctiFilePath:
        // eslint-disable-next-line max-len
        'C:/Program Files/Ultraleap/TouchFree/SettingsUI/TouchFreeSettingsUI_Data/StreamingAssets/CallToInteract/AirPush_Portrait.mp4',
    ctiHideTrigger: 1,
    ctiShowAfterTimer: 10.0,
    StartupUIShown: false,
};

export const defaultVisualsConfig: VisualsConfig = { ...defaultCursorVisualsConfig, ...defaultCtiVisualsConfig };
