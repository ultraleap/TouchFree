import styles from './Visuals.module.scss';

import classNames from 'classnames/bind';
import React, { useState, useEffect, useRef, useMemo } from 'react';
import tinycolor, { ColorFormats } from 'tinycolor2';

import { readVisualsConfig, isDesktop, writeVisualsConfig } from '@/TauriUtils';
import { useIsLandscape, useStatefulRef } from '@/customHooks';

import { SVGCursor } from 'touchfree/src/Cursors/SvgCursor';
import TouchFree from 'touchfree/src/TouchFree';

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

const VisualsScreen: React.FC = () => {
    const isLandscape = useIsLandscape();
    const cursorSection = useRef<HTMLDivElement>(null);

    const writtenConfig = useRef<VisualsConfig>();
    const config = useStatefulRef<VisualsConfig>(defaultVisualsConfig);

    const [hasReadConfig, setHasReadConfig] = useState<boolean>(false);
    const [cursor] = useState<SVGCursor>(TouchFree.GetCurrentCursor() as SVGCursor);
    const [currentPreviewBgIndex, setCurrentPreviewBgIndex] = useState<number>(0);

    useEffect(() => {
        const section = cursorSection.current?.style;
        if (!section || !hasReadConfig) return;

        const currentPreset = presets[config.current.activeCursorPreset];
        const cursorStyle = cursorStyles[currentPreset];
        section.setProperty('--center-fill', cursorStyle[0]);
        section.setProperty('--outer-fill', cursorStyle[1]);
        section.setProperty('--center-border', cursorStyle[2]);

        config.current.cursorEnabled
            ? cursorStyle.forEach((value, index) => cursor.SetColor(index, value))
            : cursor.ResetToDefaultColors();
    }, [
        hasReadConfig,
        config.current.cursorEnabled,
        config.current.activeCursorPreset,
        config.current.primaryCustomColor,
        config.current.secondaryCustomColor,
        config.current.tertiaryCustomColor,
    ]);

    const updateConfig = (content: Partial<VisualsConfig>) => {
        config.current = {...config.current, ...content};
    };

    const writeVisualsConfigIfNew = () => {
        if (writtenConfig.current === config.current) return;
        writtenConfig.current = config.current;
        writeVisualsConfig(config.current).catch((err) => console.error(err));
    }; 

    useEffect(() => {
        readVisualsConfig()
            .then((fileConfig) => {
                updateConfig(fileConfig);
                setCustomColorsFromConfig(fileConfig);
                setHasReadConfig(true);
                window.addEventListener('pointerup', writeVisualsConfigIfNew);
            })
            .catch((err) => console.error(err));

        return () => {
            cursor.ResetToDefaultColors();
            window.removeEventListener('pointerup', writeVisualsConfigIfNew);
        };
    }, []);

    if (!isDesktop() || !hasReadConfig) return <></>;

    return (
        <div className={classes('scroll-div')}>
            <label className={classes('label-container')}>
                <p className={classes('label-container__label')}>
                    Visuals affects the TouchFree Overlay application only.
                    <br />
                    To update the cursor in your application, use TouchFree Tooling
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
                                updateConfig(defaultCursorVisualsConfig);
                            }}
                        />
                    </div>
                    <LabelledToggleSwitch
                        name="Enable Cursor"
                        value={config.current.cursorEnabled}
                        onChange={(value) => updateConfig({ cursorEnabled: value })}
                    />
                    {config.current.cursorEnabled && (
                        <>
                            <div className={classes('cursor-style')}>
                                <RadioGroup
                                    name="StylePresets"
                                    selected={styleOptions.indexOf(presets[config.current.activeCursorPreset]) ?? 0}
                                    options={styleOptions}
                                    onChange={(preset) => {
                                        updateConfig({ activeCursorPreset: presets.indexOf(preset as CursorPreset) });
                                        writeVisualsConfigIfNew();
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
                            {presets[config.current.activeCursorPreset] === 'Custom' && (
                                <ColorPicker
                                    cursorStyle={cursorStyles.Custom}
                                    updateCursorStyle={(style) => {
                                        cursorStyles.Custom = style;
                                        updateConfig({
                                            primaryCustomColor: convertHexToRGBA(style[0]),
                                            secondaryCustomColor: convertHexToRGBA(style[1]),
                                            tertiaryCustomColor: convertHexToRGBA(style[2]),
                                        });
                                    }}
                                    writeVisualsConfigIfNew={writeVisualsConfigIfNew}
                                />
                            )}
                            <TextSlider
                                name="Size"
                                rangeMin={0.1}
                                rangeMax={1}
                                leftLabel="Min"
                                rightLabel="Max"
                                value={roundToTwoDP(config.current.cursorSizeCm)}
                                onChange={(value) => updateConfig({ cursorSizeCm: value })}
                            />
                            <TextSlider
                                name="Ring Thickness"
                                rangeMin={0.1}
                                rangeMax={1}
                                leftLabel="Min"
                                rightLabel="Max"
                                value={roundToTwoDP(config.current.cursorRingThickness)}
                                onChange={(value) => updateConfig({ cursorRingThickness: value })}
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
                            onClick={() => updateConfig(defaultCtiVisualsConfig)}
                        />
                    </div>
                    <LabelledToggleSwitch
                        name="Enable Call to Interact"
                        value={config.current.ctiEnabled}
                        onChange={(value) => updateConfig({ ctiEnabled: value })}
                    />
                    {config.current.ctiEnabled && (
                        <>
                            <FileInput
                                name="Call to Interact File"
                                value={config.current.ctiFilePath.split('/').pop() ?? ''}
                                acceptedExtensions={['webm', 'mp4']}
                                onFilePicked={(path) => {
                                    updateConfig({ ctiFilePath: path });
                                    writeVisualsConfigIfNew();
                                }}
                            />
                            <TextSlider
                                name="Inactivity Activation"
                                rangeMin={1}
                                rangeMax={60}
                                stepSize={1}
                                leftLabel="1 Seconds"
                                rightLabel="60 Seconds"
                                value={roundToTwoDP(config.current.ctiShowAfterTimer)}
                                onChange={(value) => updateConfig({ ctiShowAfterTimer: value }) }
                            />
                            <RadioLine
                                name="Close CTI When"
                                selected={config.current.ctiHideTrigger}
                                options={closeCtiOptions}
                                onChange={(option) => updateConfig({ ctiHideTrigger: closeCtiOptions.indexOf(option)})}
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
