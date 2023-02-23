import styles from './Visuals.module.scss';

import classNames from 'classnames/bind';
import React, { useState, useEffect, useRef, useReducer, useMemo } from 'react';
import tinycolor, { ColorFormats } from 'tinycolor2';

import { readVisualsConfig, isDesktop, writeVisualsConfig } from '@/TauriUtils';
import { useIsLandscape } from '@/customHooks';

import {
    BlackTextBg,
    BlackTextBgPreview,
    GradientBg,
    GradientBgPreview,
    MountainBg,
    MountainBgPreview,
    WhiteTextBg,
    WhiteTextBgPreview,
} from '@/Images';

import {
    DocsLink,
    FileInput,
    LabelledToggleSwitch,
    OutlinedTextButton,
    RadioGroup,
    RadioLine,
    TextSlider,
} from '@/Components';

import ColorPicker from './ColorPicker';
import { defaultCtiVisualsConfig } from './VisualsUtils';
import {
    CursorPreset,
    cursorStyles,
    VisualsConfig,
    defaultVisualsConfig,
    CursorStyle,
    presets,
    defaultCursorVisualsConfig,
} from './VisualsUtils';

const classes = classNames.bind(styles);

const styleOptions = Object.keys(cursorStyles);
const bgImages = [GradientBg, WhiteTextBg, BlackTextBg, MountainBg];
const bgPreviewImages = [GradientBgPreview, WhiteTextBgPreview, BlackTextBgPreview, MountainBgPreview];
const closeCtiOptions = ['Users Hand Present', 'User Performs Interaction'];

const reducer = (state: VisualsConfig, payload: { content: Partial<VisualsConfig> }) => {
    const newState: VisualsConfig = { ...state, ...payload.content };
    return newState;
};

const VisualsScreen: React.FC = () => {
    const isLandscape = useIsLandscape();
    const cursorSection = useRef<HTMLDivElement>(null);

    const [state, dispatch] = useReducer(reducer, defaultVisualsConfig);
    const [hasReadConfig, setHasReadConfig] = useState<boolean>(false);

    const [currentPreviewBgIndex, setCurrentPreviewBgIndex] = useState<number>(0);

    const updateCursorPreview = (cursorStyle: CursorStyle) => {
        const section = cursorSection.current?.style;
        if (!section) return;

        section.setProperty('--center-fill', cursorStyle[0]);
        section.setProperty('--outer-fill', cursorStyle[1]);
        section.setProperty('--center-border', cursorStyle[2]);
    };

    let writtenConfig: VisualsConfig;
    const writeVisualsConfigIfNew = () => {
        if (writtenConfig == state) return;
        writtenConfig = state;
        writeVisualsConfig(state).catch((err) => console.error(err));
    };

    const currentPreset = useMemo((): CursorPreset => {
        return presets[state.activeCursorPreset];
    }, [state.activeCursorPreset]);

    useEffect(() => {
        readVisualsConfig()
            .then((fileConfig) => {
                dispatch({ content: fileConfig });
                setCustomColorsFromConfig(fileConfig);
                setHasReadConfig(true);
                updateCursorPreview(cursorStyles[presets[fileConfig.activeCursorPreset]]);
            })
            .catch((err) => console.error(err));
    }, []);

    if (!isDesktop() || !hasReadConfig) return <></>;

    window.onpointerup = () => {
        writeVisualsConfigIfNew();
    };
    window.onpointerout = () => {
        writeVisualsConfigIfNew();
    };

    return (
        <div className={classes('scroll-div')}>
            <label className={classes('label-container')}>
                <p className={classes('label-container__label')}>
                    Visuals affects Overlay application only.
                    <br />
                    To update the cursor in web, use TouchFree Tooling
                </p>
                <DocsLink title={'Find out More'} url={'https://developer.leapmotion.com/touchfree-tooling-for-web'} />
            </label>
            <div className={classes('container')}>
                <div ref={cursorSection} className={classes('section')}>
                    <div className={classes('title-line')}>
                        <h1> Cursor Styles </h1>
                        <OutlinedTextButton
                            title="Reset to Default"
                            onClick={() => {
                                dispatch({ content: defaultCursorVisualsConfig });
                                updateCursorPreview(
                                    cursorStyles[presets[defaultCursorVisualsConfig.activeCursorPreset]]
                                );
                            }}
                        />
                    </div>
                    <LabelledToggleSwitch
                        name="Enable Cursor"
                        value={state.cursorEnabled}
                        onChange={(value) => dispatch({ content: { cursorEnabled: value }})}
                    />
                    {state.cursorEnabled && (
                        <>
                            <div className={classes('cursor-style')}>
                                <RadioGroup
                                    name="StylePresets"
                                    selected={styleOptions.indexOf(currentPreset) ?? 0}
                                    options={styleOptions}
                                    onChange={(preset) => {
                                        dispatch({ 
                                            content: { activeCursorPreset: presets.indexOf(preset as CursorPreset) }
                                        });
                                        updateCursorPreview(cursorStyles[preset as CursorPreset]);
                                    }}
                                />
                                <div
                                    className={classes('cursor-style__preview')}
                                    style={{ backgroundImage: `url(${bgImages[currentPreviewBgIndex]})` }}
                                >
                                    <div className={classes('cursor-style__preview__cursor')} />
                                    <div className={classes('cursor-style__preview__bg-selector')}>
                                        {bgPreviewImages.map((src, index) => (
                                            <img
                                                key={index}
                                                className={classes('cursor-style__preview__bg-selector__img', {
                                                    'cursor-style__preview__bg-selector__img--active':
                                                        index === currentPreviewBgIndex,
                                                })}
                                                onClick={() => setCurrentPreviewBgIndex(index)}
                                                src={src}
                                            />
                                        ))}
                                    </div>
                                </div>
                            </div>
                            {currentPreset === 'Custom' && (
                                <ColorPicker
                                    cursorStyle={cursorStyles.Custom}
                                    updateCursorStyle={(style) => {
                                        cursorStyles.Custom = style;
                                        dispatch({ 
                                            content: {
                                                primaryCustomColor: convertHexToRGBA(style[0]),
                                                secondaryCustomColor: convertHexToRGBA(style[1]),
                                                tertiaryCustomColor: convertHexToRGBA(style[2]),
                                            }
                                        });
                                        updateCursorPreview(style);
                                    }}
                                />
                            )}
                            <TextSlider
                                name="Size (cm)"
                                rangeMin={0.1}
                                rangeMax={1}
                                leftLabel="Min"
                                rightLabel="Max"
                                value={roundToTwoDP(state.cursorSizeCm)}
                                onChange={(value) => dispatch({ content: { cursorSizeCm: value }})}
                            />
                            <TextSlider
                                name="Ring Thickness (cm)"
                                rangeMin={0.05}
                                rangeMax={0.6}
                                leftLabel="Min"
                                rightLabel="Max"
                                value={roundToTwoDP(state.cursorRingThickness)}
                                onChange={(value) => dispatch({ content: { cursorRingThickness: value }})}
                            />
                        </>
                    )}
                </div>
                <div className={classes('page-divider', { 'page-divider--vertical': isLandscape })} />
                <div className={classes('section')}>
                    <div className={classes('title-line')}>
                        <h1> Call to Interact </h1>
                        <OutlinedTextButton
                            title="Reset to Default"
                            onClick={() => dispatch({ content: defaultCtiVisualsConfig })}
                        />
                    </div>
                    <LabelledToggleSwitch
                        name="Enable Call to Interact"
                        value={state.ctiEnabled}
                        onChange={(value) => dispatch({ content: { ctiEnabled: value }})}
                    />
                    {state.ctiEnabled && (
                        <>
                            <FileInput
                                name="Call to Interact File"
                                value={state.ctiFilePath.split('/').pop() ?? ''}
                                acceptedExtensions={['webm', 'mp4']}
                                onFilePicked={(path) => dispatch({ content: { ctiFilePath: path }}) }
                            />
                            <TextSlider
                                name="Inactivity Activation"
                                rangeMin={1}
                                rangeMax={60}
                                stepSize={1}
                                leftLabel="1 Seconds"
                                rightLabel="60 Seconds"
                                value={roundToTwoDP(state.ctiShowAfterTimer)}
                                onChange={(value) => dispatch({ content: { ctiShowAfterTimer: value }}) }
                            />
                            <RadioLine
                                name="Close CTI When"
                                selected={state.ctiHideTrigger}
                                options={closeCtiOptions}
                                onChange={(option) => dispatch({ 
                                    content: { ctiHideTrigger: closeCtiOptions.indexOf(option) 
                                }})}
                            />
                        </>
                    )}
                </div>
            </div>
        </div>
    );
};

const roundToTwoDP = (numberIn: number) => Math.round(numberIn * 100) / 100;

const setCustomColorsFromConfig = (config: VisualsConfig) => {
    const { primaryCustomColor, secondaryCustomColor, tertiaryCustomColor } = config;
    cursorStyles.Custom = [primaryCustomColor, secondaryCustomColor, tertiaryCustomColor].map(
        (color) => '#' + tinycolor.fromRatio(color).toHex()
    ) as CursorStyle;
};

const convertHexToRGBA = (hex: string): ColorFormats.RGBA => {
    const rgba = tinycolor(hex).toRgb();
    return { r: rgba.r / 255, g: rgba.g / 255, b: rgba.b / 255, a: rgba.a };
};

export default VisualsScreen;
