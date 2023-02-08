import styles from './ManualSetup.module.scss';

import classnames from 'classnames/bind';
import { FormEvent, useEffect, useReducer } from 'react';

import { useIsLandscape } from '@/customHooks';

import { ConfigurationManager } from 'touchfree/src/Configuration/ConfigurationManager';
import { PhysicalConfig } from 'touchfree/src/Configuration/ConfigurationTypes';
import { ConnectionManager } from 'touchfree/src/Connection/ConnectionManager';
import { ConfigState } from 'touchfree/src/Connection/TouchFreeServiceTypes';

import { BackButton, TextEntry } from '@/Components';

const classes = classnames.bind(styles);

interface PhysicalState {
    screenHeight: number;
    cameraHeight: number;
    cameraLeftToRight: number;
    screenTilt: number;
    cameraRotation: number;
    cameraDistanceFromScreen: number;
    physicalConfig: PhysicalConfig;
    selectedView: string;
}

const initalState: PhysicalState = {
    screenHeight: 0,
    cameraHeight: 0,
    cameraLeftToRight: 0,
    screenTilt: 0,
    cameraRotation: 0,
    cameraDistanceFromScreen: 0,
    physicalConfig: {
        LeapPositionRelativeToScreenBottomM: { X: 0, Y: 0, Z: 0 },
        LeapRotationD: { X: 0, Y: 0, Z: 0 },
        ScreenHeightM: 0,
        ScreenRotationD: 0,
        ScreenHeightPX: 0,
        ScreenWidthPX: 0,
    },
    selectedView: 'screenHeight',
};

const reducer = (state: PhysicalState, content: Partial<PhysicalState>) => {
    const newState = { ...state, ...content };
    let xRotation = newState.cameraRotation % 360;
    xRotation = xRotation > 90 ? 180 - xRotation : xRotation < -90 ? -xRotation - 180 : xRotation;
    const config: PhysicalConfig = {
        LeapPositionRelativeToScreenBottomM: {
            X: newState.cameraLeftToRight / 100,
            Y: newState.cameraHeight / 100,
            Z: -newState.cameraDistanceFromScreen / 100,
        },
        LeapRotationD: {
            ...newState.physicalConfig.LeapRotationD,
            X: xRotation,
            Z: newState.cameraRotation > 90 || newState.cameraRotation < -90 ? 180 : 0,
        },
        ScreenHeightM: newState.screenHeight / 100,
        ScreenRotationD: newState.screenTilt,
        ScreenHeightPX: window.screen.height * window.devicePixelRatio,
        ScreenWidthPX: window.screen.width * window.devicePixelRatio,
    };
    ConfigurationManager.RequestConfigFileChange(null, config, (result) => {
        if (result.status !== 'Success') {
            console.error(`Failed to set config state! Info: ${result.message}`);
        }
    });
    return newState as PhysicalState;
};

const ManualSetupScreen = () => {
    const [state, dispatch] = useReducer(reducer, initalState);
    const isLandscape = useIsLandscape();

    useEffect(() => {
        ConnectionManager.AddConnectionListener(() => {
            ConfigurationManager.RequestConfigFileState(setStateFromFile);
        });
    }, []);

    const setStateFromFile = (config: ConfigState) => {
        let cameraRotation =
            Math.round(config.physical.LeapRotationD.Z) === 180
                ? -config.physical.LeapRotationD.X - config.physical.LeapRotationD.Z
                : config.physical.LeapRotationD.X;
        cameraRotation = cameraRotation <= -180 ? cameraRotation + 360 : cameraRotation;

        dispatch({
            screenHeight: roundToFiveDecimals(config.physical.ScreenHeightM * 100),
            cameraHeight: roundToFiveDecimals(config.physical.LeapPositionRelativeToScreenBottomM.Y * 100),
            cameraLeftToRight: roundToFiveDecimals(config.physical.LeapPositionRelativeToScreenBottomM.X * 100),
            cameraDistanceFromScreen: roundToFiveDecimals(-config.physical.LeapPositionRelativeToScreenBottomM.Z * 100),
            cameraRotation,
            physicalConfig: config.physical,
            screenTilt: config.physical.ScreenRotationD,
        });
    };

    const roundToFiveDecimals = (numberIn: number) => {
        return Math.round(numberIn * 100000) / 100000;
    };

    const update = (key: keyof PhysicalState, event: FormEvent<HTMLInputElement>) => {
        dispatch({ [key]: parseFloat(event.currentTarget?.value) });
    };

    return (
        <div className={classes('container')}>
            <div className={classes('header')}>
                <div className={classes('title-line')}>
                    <h1> Manual Calibration </h1>
                </div>
                {isLandscape ? <></> : <BackButton />}
            </div>

            <div className={classes('sub-container')}>
                <div className={classes('options-container')}>
                    <div className={classes('options-container--row')}>
                        <TextEntry
                            name="Screen Height (cm)"
                            value={state.screenHeight.toString()}
                            onChange={(e) => update('screenHeight', e)}
                            onPointerDown={() => dispatch({ selectedView: 'screenHeight' })}
                            selected={state.selectedView === 'screenHeight'}
                        />
                        <TextEntry
                            name="Camera Height (cm)"
                            value={state.cameraHeight.toString()}
                            onChange={(e) => update('cameraHeight', e)}
                            onPointerDown={() => dispatch({ selectedView: 'cameraHeight' })}
                            selected={state.selectedView === 'cameraHeight'}
                        />
                        <TextEntry
                            name="Camera Left to Right (cm)"
                            value={state.cameraLeftToRight.toString()}
                            onChange={(e) => update('cameraLeftToRight', e)}
                            onPointerDown={() => dispatch({ selectedView: 'cameraLeftToRight' })}
                            selected={state.selectedView === 'cameraLeftToRight'}
                        />
                    </div>
                    <div className={classes('sub-col')}>
                        <TextEntry
                            name="Screen Tilt (degrees)"
                            value={state.screenTilt.toString()}
                            onChange={(e) => update('screenTilt', e)}
                            onPointerDown={() => dispatch({ selectedView: 'screenTilt' })}
                            selected={state.selectedView === 'screenTilt'}
                        />
                        <TextEntry
                            name="Camera Rotation (degrees)"
                            value={state.cameraRotation.toString()}
                            onChange={(e) => update('cameraRotation', e)}
                            onPointerDown={() => dispatch({ selectedView: 'cameraRotation' })}
                            selected={state.selectedView === 'cameraRotation'}
                        />
                        <TextEntry
                            name="Camera Distance from Screen (cm)"
                            value={state.cameraDistanceFromScreen.toString()}
                            onChange={(e) => update('cameraDistanceFromScreen', e)}
                            onPointerDown={() => dispatch({ selectedView: 'cameraDistanceFromScreen' })}
                            selected={state.selectedView === 'cameraDistanceFromScreen'}
                        />
                    </div>
                </div>
                <div className={classes('image-container')}>
                    <div className={classes('screen-images', state.selectedView)}>
                        <div className={classes('screen-container')}>
                            <div className={classes('screen-mock')}>
                                <div className={classes('screenFrontOutside')}>
                                    <div className={classes('screenFrontCamera')}>
                                        <div className={classes('screenFrontCameraBottom')}></div>
                                    </div>
                                    <div className={classes('screenFrontInside')}>
                                        <div className={classes('screenFrontTopLine')}></div>
                                        <div className={classes('screenFrontCenterLineVert')}></div>
                                        <div className={classes('screenFrontBottomLine')}></div>
                                    </div>
                                </div>
                            </div>
                            <p className={classes('screen-label')}>Front View</p>
                        </div>
                        <div className={classes('screen-container')}>
                            <div className={classes('screen-mock')}>
                                <div className={classes('screenSideCamera')}>
                                    <div className={classes('screenSideCameraBottom')}></div>
                                </div>
                                <div className={classes('screenSideOutside')}></div>
                                <div className={classes('screenSideInside')}></div>
                                <div className={classes('screenSideCenterLineVert')}></div>
                                <div className={classes('screenSideExtraLineVert')}></div>
                                <div className={classes('screenSideTopLine')}></div>
                                <div className={classes('screenSideBottomLine')}></div>
                                <div className={classes('screenSideTiltBox')}></div>
                            </div>
                            <p className={classes('screen-label')}>Side View</p>
                        </div>
                    </div>
                    {isLandscape ? <BackButton /> : <></>}
                </div>
            </div>
        </div>
    );
};

export default ManualSetupScreen;
