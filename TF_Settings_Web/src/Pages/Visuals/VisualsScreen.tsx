import rgbaToHex from 'rgb-hex';

import styles from './Visuals.module.scss';

import classNames from 'classnames/bind';
import React, { useState, useEffect, useRef, useReducer } from 'react';

import { readVisualsConfig, isDesktop, writeVisualsConfig } from '@/TauriUtils';
import { useStatefulRef } from '@/customHooks';

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
import {
    CursorStylePresets,
    cursorStylePresets,
    VisualsConfig,
    defaultVisualsConfig,
    CursorStyle,
} from './VisualsUtils';

const classes = classNames.bind(styles);

const styleOptions = Object.keys(cursorStylePresets);
const bgImages = [GradientBg, WhiteTextBg, BlackTextBg, MountainBg];
const bgPreviewImages = [GradientBgPreview, WhiteTextBgPreview, BlackTextBgPreview, MountainBgPreview];
const closeCtiOptions = ['Users Hand Present', 'User Performs Interaction'];

const VisualsScreen: React.FC = () => {
    const previewContainer = useRef<HTMLDivElement>(null);

    const reducer = (state: VisualsConfig, content: Partial<VisualsConfig>) => {
        const newState: VisualsConfig = { ...state, ...content };
        stateRef.current = newState;
        return newState;
    };
    const stateRef = useRef<VisualsConfig>(defaultVisualsConfig);
    const [state, dispatch] = useReducer(reducer, defaultVisualsConfig);
    const [hasReadConfig, setHasReadConfig] = useState<boolean>(false);

    const currentStyle = useStatefulRef<CursorStylePresets>('Solid (Light)');
    const [currentPreviewBgIndex, setCurrentPreviewBgIndex] = useState<number>(0);

    // Store this in the cursor defaults???
    const customCursorColors = useStatefulRef<CursorStyle>(['#f8b195ff', '#f67280ff', '#6c5b7bff']);

    const cursorColors = useStatefulRef<CursorStyle>(
        cursorStylePresets[currentStyle.current] ?? customCursorColors.current
    );

    useEffect(() => {
        const style = previewContainer.current?.style;
        if (!style) return;

        style.setProperty('--center-fill', cursorColors.current[0]);
        style.setProperty('--outer-fill', cursorColors.current[1]);
        style.setProperty('--center-border', cursorColors.current[2]);
    }, [cursorColors.current]);

    useEffect(() => {
        cursorColors.current = cursorStylePresets[currentStyle.current] ?? customCursorColors.current;
    }, [currentStyle.current]);

    useEffect(() => {
        readVisualsConfig()
            .then((fileConfig) => {
                dispatch(fileConfig);
                setHasReadConfig(true);
                getStylePresetFromState(fileConfig);
            })
            .catch((err) => console.error(err));

        return () => {
            writeVisualsConfig(stateRef.current);
        };
    }, []);

    if (!isDesktop() || !hasReadConfig) return <></>;

    return (
        <div className={classes('scroll-div')}>
            <div className={classes('container')}>
                <label className={classes('label-container')}>
                    <p className={classes('label-container__label')}>
                        Visuals affects Overlay application only.
                        <br />
                        To update the cursor in web, use TouchFree Tooling
                    </p>
                    <DocsLink
                        title={'Find out More'}
                        url={'https://developer.leapmotion.com/touchfree-tooling-for-web'}
                    />
                </label>
                <div className={classes('title-line')}>
                    <h1> Cursor Styles </h1>
                    <OutlinedTextButton title="Reset to Default" onClick={() => console.log('RESET STYLES')} />
                </div>
                <div className={classes('section')}>
                    <LabelledToggleSwitch
                        name="Enable Cursor"
                        value={state.cursorEnabled}
                        onChange={(value) => dispatch({ cursorEnabled: value })}
                    />
                    {state.cursorEnabled && (
                        <>
                            <div className={classes('two-cols')}>
                                <RadioGroup
                                    name="StylePresets"
                                    selected={styleOptions.indexOf(currentStyle.current) ?? 0}
                                    options={styleOptions}
                                    onChange={(preset) => {
                                        currentStyle.current = preset as CursorStylePresets;
                                    }}
                                />
                                <div
                                    className={classes('cursor-preview')}
                                    style={{ backgroundImage: `url(${bgImages[currentPreviewBgIndex]})` }}
                                >
                                    <div ref={previewContainer} className={classes('cursor-preview__cursor')} />
                                    <div className={classes('cursor-preview__bg-selector')}>
                                        {bgPreviewImages.map((src, index) => (
                                            <img
                                                key={index}
                                                className={classes('cursor-preview__bg-selector__img', {
                                                    'cursor-preview__bg-selector__img--active':
                                                        index === currentPreviewBgIndex,
                                                })}
                                                onClick={() => setCurrentPreviewBgIndex(index)}
                                                src={src}
                                            />
                                        ))}
                                    </div>
                                </div>
                            </div>
                            {currentStyle.current === 'Custom' && (
                                <ColorPicker
                                    cursorColors={cursorColors.current}
                                    updateCursorColors={(colors) =>
                                        (customCursorColors.current = cursorColors.current = colors)
                                    }
                                />
                            )}
                            <TextSlider
                                name="Size (cm)"
                                rangeMin={0.1}
                                rangeMax={1}
                                leftLabel="Min"
                                rightLabel="Max"
                                value={state.cursorSizeCm}
                                onChange={(value) => dispatch({ cursorSizeCm: value })}
                            />
                            <TextSlider
                                name="Ring Thickness (cm)"
                                rangeMin={0.05}
                                rangeMax={0.6}
                                leftLabel="Min"
                                rightLabel="Max"
                                value={state.cursorRingThickness}
                                onChange={(value) => dispatch({ cursorRingThickness: value })}
                            />
                        </>
                    )}
                </div>
                <div className={classes('page-divider')} />
                <div className={classes('title-line')}>
                    <h1> Call to Interact </h1>
                    <OutlinedTextButton title="Reset to Default" onClick={() => console.log('RESET CTI')} />
                </div>
                <div className={classes('section')}>
                    <LabelledToggleSwitch
                        name="Enable Call to Interact"
                        value={state.ctiEnabled}
                        onChange={(value) => dispatch({ ctiEnabled: value })}
                    />
                    {state.ctiEnabled && (
                        <>
                            <FileInput
                                name="Call to Interact File"
                                value="1 Push in mid-air to start.mp4"
                                acceptedFileTypes="video/*"
                                onChange={(e) => console.log(e.target.value)}
                            />
                            <TextSlider
                                name="Inactivity Activation"
                                rangeMin={1}
                                rangeMax={60}
                                stepSize={1}
                                leftLabel="1 Seconds"
                                rightLabel="60 Seconds"
                                value={state.ctiShowAfterTimer}
                                onChange={(value) => dispatch({ ctiShowAfterTimer: value })}
                            />
                            <RadioLine
                                name="Close CTI When"
                                selected={state.ctiHideTrigger}
                                options={closeCtiOptions}
                                onChange={(option) => dispatch({ ctiHideTrigger: closeCtiOptions.indexOf(option) })}
                            />
                        </>
                    )}
                </div>
            </div>
        </div>
    );
};

const getStylePresetFromState = (state: VisualsConfig): CursorStylePresets => {
    const { activeCursorPreset, primaryCustomColor, secondaryCustomColor, tertiaryCustomColor } = state;
    if (activeCursorPreset === 0) return 'Solid (Light)';
    if (activeCursorPreset === 1) return 'Solid (Dark)';

    const stateCursorStyle = [primaryCustomColor, secondaryCustomColor, tertiaryCustomColor].map(
        (color) => '#' + rgbaToHex(color.r * 255, color.g * 255, color.b * 255, color.a)
    ) as CursorStyle;

    if (isCursorStyleEqual(stateCursorStyle, cursorStylePresets['Outline (Light)'])) return 'Outline (Light)';
    if (isCursorStyleEqual(stateCursorStyle, cursorStylePresets['Outline (Dark)'])) return 'Outline (Dark)';

    return 'Custom';
};
const isCursorStyleEqual = (a: CursorStyle, b?: CursorStyle) => b && a[0] === b[0] && a[1] === b[1] && a[2] === b[2];

export default VisualsScreen;
