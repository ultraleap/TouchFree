import styles from './Visuals.module.scss';

import classNames from 'classnames/bind';
import React, { useState } from 'react';

import { useIsLinux } from '@/customHooks';

import {
    DocsLink,
    FileInput,
    LabelledToggleSwitch,
    OutlinedTextButton,
    RadioGroup,
    RadioLine,
    TextSlider,
} from '@/Components';

const classes = classNames.bind(styles);

const StylePresets: string[] = ['Recommended (Light)', 'Recommended (Dark)', 'Solid (Light)', 'Solid (Dark)', 'Custom'];
const CloseCtiOptions: string[] = ['Users Hand Present', 'User Performs Interaction'];

const VisualsScreen: React.FC = () => {
    if (useIsLinux()) return <></>;

    const [currentStyleIndex, setCurrentStyleIndex] = useState<number>(0);
    const [size, setSize] = useState<number>(0.5);
    const [ringThickness, setRingThickness] = useState<number>(0.15);
    const [ctiEnabled, setCtiEnabled] = useState<boolean>(true);
    const [ctiPath, setCtiPath] = useState<boolean>(true);
    const [ctiTriggerTime, setCtiTriggerTime] = useState<number>(10);
    const [ctiCloseOptionIndex, setCtiCloseOptionIndex] = useState<number>(0);

    return (
        <div className={classes('container')}>
            <div className={classes('title-line')}>
                <h1> Cursor Styles </h1>
                <OutlinedTextButton title="Reset to Default" onClick={() => console.log('RESET STYLES')} />
            </div>
            <label className={classes('label-container')}>
                <p className={classes('label-container__label')}>
                    Visuals affects Overlay application only.
                    <br />
                    To update the cursor in web, use TouchFree Tooling
                </p>
                <DocsLink title={'Find out More'} url={'https://developer.leapmotion.com/touchfree-tooling-for-web'} />
            </label>
            <div className={classes('section')}>
                <div className={classes('two-cols')}>
                    <RadioGroup
                        name="StylePresets"
                        selected={currentStyleIndex}
                        options={StylePresets}
                        onChange={(preset) => setCurrentStyleIndex(StylePresets.indexOf(preset))}
                    />
                    <div className={classes('cursor-preview')}></div>
                </div>
                <TextSlider
                    name="Size (cm)"
                    rangeMin={0.1}
                    rangeMax={1}
                    leftLabel="Min"
                    rightLabel="Max"
                    value={size}
                    onChange={setSize}
                />
                <TextSlider
                    name="Ring Thickness (cm)"
                    rangeMin={0.05}
                    rangeMax={0.6}
                    leftLabel="Min"
                    rightLabel="Max"
                    value={ringThickness}
                    onChange={setRingThickness}
                />
            </div>
            <div className={classes('page-divider')} />
            <div className={classes('title-line')}>
                <h1> Call to Interact </h1>
                <OutlinedTextButton title="Reset to Default" onClick={() => console.log('RESET CTI')} />
            </div>
            <div className={classes('section')}>
                <LabelledToggleSwitch name="Enable Call to Interact" value={ctiEnabled} onChange={setCtiEnabled} />
                {ctiEnabled && (
                    <>
                        <FileInput
                            name="Call to Interact File"
                            value="1 Push in mid-air to start.mp4"
                            acceptedFileTypes="video/*"
                            onChange={(e) => console.log(e.target.value)}
                        />
                        <TextSlider
                            name="Inactivity Activation"
                            rangeMin={5}
                            rangeMax={60}
                            stepSize={1}
                            leftLabel="5 Seconds"
                            rightLabel="60 Seconds"
                            value={ctiTriggerTime}
                            onChange={setCtiTriggerTime}
                        />
                        <RadioLine
                            name="Close CTI When"
                            selected={ctiCloseOptionIndex}
                            options={CloseCtiOptions}
                            onChange={(option) => setCtiCloseOptionIndex(CloseCtiOptions.indexOf(option))}
                        />
                    </>
                )}
            </div>
        </div>
    );
};

export default VisualsScreen;
