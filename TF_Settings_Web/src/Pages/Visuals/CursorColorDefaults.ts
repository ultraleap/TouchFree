export const cursorSections = ['Outer Fill', 'Center Fill', 'Center Border'] as const;

export type CursorSectionColors = {
    [Property in (typeof cursorSections)[number]]: string;
};

const presets = ['Recommended (Light)', 'Recommended (Dark)', 'Solid (Light)', 'Solid (Dark)', 'Custom'] as const;

export type StyleDefaults = (typeof presets)[number];

export const styleDefaults: { [Property in (typeof presets)[number]]: CursorSectionColors | undefined } = {
    'Recommended (Light)': {
        'Center Border': '#ffffff',
        'Center Fill': '#ffffff00',
        'Outer Fill': '#ffffff',
    },
    'Recommended (Dark)': {
        'Center Border': '#000000',
        'Center Fill': '#00000000',
        'Outer Fill': '#000000',
    },
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
    Custom: undefined,
};
