import classNames from 'classnames/bind';

import styles from './ManualSetup.module.scss';

import { Component } from 'react';

import { ConfigurationManager } from 'TouchFree/src/Configuration/ConfigurationManager';
import { PhysicalConfig } from 'TouchFree/src/Configuration/ConfigurationTypes';
import { ConnectionManager } from 'TouchFree/src/Connection/ConnectionManager';
import { ConfigState, WebSocketResponse } from 'TouchFree/src/Connection/TouchFreeServiceTypes';

import { TextEntry } from '@/Components';

const classes = classNames.bind(styles);

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

export class ManualSetupScreen extends Component<{}, PhysicalState> {
    currentScreenHeight = '';
    currentCameraHeight = '';
    currentCameraLeftToRight = '';
    currentScreenTilt = '';
    currentCameraRotation = '';
    currentCameraDistanceFromScreen = '';

    constructor(props: {}) {
        super(props);

        const state: PhysicalState = {
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

        this.state = state;
    }

    componentDidMount(): void {
        ConnectionManager.AddConnectionListener(() => {
            ConfigurationManager.RequestConfigFileState(this.setStateFromFile.bind(this));
        });
    }

    setStateFromFile(config: ConfigState): void {
        let cameraRotation =
            Math.round(config.physical.LeapRotationD.Z) === 180
                ? -config.physical.LeapRotationD.X - config.physical.LeapRotationD.Z
                : config.physical.LeapRotationD.X;
        cameraRotation = cameraRotation <= -180 ? cameraRotation + 360 : cameraRotation;

        const stateUpdate = {
            screenHeight: this.roundToFiveDecimals(config.physical.ScreenHeightM * 100),
            cameraHeight: this.roundToFiveDecimals(config.physical.LeapPositionRelativeToScreenBottomM.Y * 100),
            cameraLeftToRight: this.roundToFiveDecimals(config.physical.LeapPositionRelativeToScreenBottomM.X * 100),
            cameraDistanceFromScreen: this.roundToFiveDecimals(
                -config.physical.LeapPositionRelativeToScreenBottomM.Z * 100
            ),
            cameraRotation,
            physicalConfig: config.physical,
            screenTilt: config.physical.ScreenRotationD,
        };

        this.currentScreenHeight = stateUpdate.screenHeight.toString();
        this.currentCameraHeight = stateUpdate.cameraHeight.toString();
        this.currentCameraLeftToRight = stateUpdate.cameraLeftToRight.toString();
        this.currentScreenTilt = stateUpdate.screenTilt.toString();
        this.currentCameraRotation = stateUpdate.cameraRotation.toString();
        this.currentCameraDistanceFromScreen = stateUpdate.cameraDistanceFromScreen.toString();

        this.setState(stateUpdate);
    }

    roundToFiveDecimals(numberIn: number): number {
        return Math.round(numberIn * 100000) / 100000;
    }

    componentDidUpdate(_prevProps: {}, prevState: PhysicalState): void {
        if (this.state !== null && this.state !== prevState) {
            let xRotation = this.state.cameraRotation % 360;
            xRotation = xRotation > 90 ? 180 - xRotation : xRotation < -90 ? -xRotation - 180 : xRotation;

            const physicalConfigState: PhysicalConfig = {
                ...this.state.physicalConfig,
                LeapPositionRelativeToScreenBottomM: {
                    X: this.state.cameraLeftToRight / 100,
                    Y: this.state.cameraHeight / 100,
                    Z: -this.state.cameraDistanceFromScreen / 100,
                },
                LeapRotationD: {
                    ...this.state.physicalConfig.LeapRotationD,
                    X: xRotation,
                    Z: this.state.cameraRotation > 90 || this.state.cameraRotation < -90 ? 180 : 0,
                },
                ScreenHeightM: this.state.screenHeight / 100,
                ScreenRotationD: this.state.screenTilt,
                ScreenHeightPX: window.screen.height * window.devicePixelRatio,
                ScreenWidthPX: window.screen.width * window.devicePixelRatio,
            };
            ConfigurationManager.RequestConfigFileChange(
                null,
                physicalConfigState,
                this.configChangeCbHandler.bind(this)
            );
        }
    }

    configChangeCbHandler(result: WebSocketResponse): void {
        if (result.status !== 'Success') {
            console.error(`Failed to set config state! Info: ${result.message}`);
        }
    }

    onScreenHeightChanged(e: React.FormEvent<HTMLInputElement>): void {
        this.currentScreenHeight = e.currentTarget?.value;
        const value = Number.parseFloat(e.currentTarget?.value);
        if (!Number.isNaN(value)) {
            this.setState({ screenHeight: value });
        }
    }

    onCameraHeightChanged(e: React.FormEvent<HTMLInputElement>): void {
        this.currentCameraHeight = e.currentTarget?.value;
        const value = Number.parseFloat(e.currentTarget?.value);
        if (!Number.isNaN(value)) {
            this.setState({ cameraHeight: value });
        }
    }

    onCameraLeftToRightChanged(e: React.FormEvent<HTMLInputElement>): void {
        this.currentCameraLeftToRight = e.currentTarget?.value;
        const value = Number.parseFloat(e.currentTarget?.value);
        if (!Number.isNaN(value)) {
            this.setState({ cameraLeftToRight: value });
        }
    }

    onScreenTiltChanged(e: React.FormEvent<HTMLInputElement>): void {
        this.currentScreenTilt = e.currentTarget?.value;
        const value = Number.parseFloat(e.currentTarget?.value);
        if (!Number.isNaN(value)) {
            this.setState({ screenTilt: value });
        }
    }

    onCameraRotationChanged(e: React.FormEvent<HTMLInputElement>): void {
        this.currentCameraRotation = e.currentTarget?.value;
        const value = Number.parseFloat(e.currentTarget?.value);
        if (!Number.isNaN(value)) {
            this.setState({ cameraRotation: value });
        }
    }

    onCameraDistanceFromScreenChanged(e: React.FormEvent<HTMLInputElement>): void {
        this.currentCameraDistanceFromScreen = e.currentTarget?.value;
        const value = Number.parseFloat(e.currentTarget?.value);
        if (!Number.isNaN(value)) {
            this.setState({ cameraDistanceFromScreen: value });
        }
    }

    onScreenHeightClicked(): void {
        this.updateSelectedView('screenHeight');
    }

    onCameraHeightClicked(): void {
        this.updateSelectedView('cameraHeight');
    }

    onCameraLeftToRightClicked(): void {
        this.updateSelectedView('cameraLeftToRight');
    }

    onScreenTiltClicked(): void {
        this.updateSelectedView('screenTilt');
    }

    onCameraRotationClicked(): void {
        this.updateSelectedView('cameraRotation');
    }

    onCameraDistanceFromScreenClicked(): void {
        this.updateSelectedView('cameraDistanceFromScreen');
    }

    updateSelectedView(selectedView: string): void {
        if (this.state.selectedView !== selectedView) {
            this.setState({ selectedView });
        }
    }

    render() {
        return (
            <div className={classes('container')}>
                <div className={classes('title-line')}>
                    <h1> Manual Calibration </h1>
                </div>

                <div className={classes('sub-container')}>
                    <div className={classes('col')}>
                        <div className={classes('sub-col')}>
                            <TextEntry
                                name="Screen Height (cm)"
                                value={this.currentScreenHeight}
                                onChange={this.onScreenHeightChanged.bind(this)}
                                onClick={this.onScreenHeightClicked.bind(this)}
                                onPointerDown={this.onScreenHeightClicked.bind(this)}
                                selected={this.state.selectedView === 'screenHeight'}
                            />
                            <TextEntry
                                name="Camera Height (cm)"
                                value={this.currentCameraHeight}
                                onChange={this.onCameraHeightChanged.bind(this)}
                                onClick={this.onCameraHeightClicked.bind(this)}
                                onPointerDown={this.onCameraHeightClicked.bind(this)}
                                selected={this.state.selectedView === 'cameraHeight'}
                            />
                            <TextEntry
                                name="Camera Left to Right (cm)"
                                value={this.currentCameraLeftToRight}
                                onChange={this.onCameraLeftToRightChanged.bind(this)}
                                onClick={this.onCameraLeftToRightClicked.bind(this)}
                                onPointerDown={this.onCameraLeftToRightClicked.bind(this)}
                                selected={this.state.selectedView === 'cameraLeftToRight'}
                            />
                        </div>
                        <div className={classes('sub-col')}>
                            <TextEntry
                                name="Screen Tilt (degrees)"
                                value={this.currentScreenTilt}
                                onChange={this.onScreenTiltChanged.bind(this)}
                                onClick={this.onScreenTiltClicked.bind(this)}
                                onPointerDown={this.onScreenTiltClicked.bind(this)}
                                selected={this.state.selectedView === 'screenTilt'}
                            />
                            <TextEntry
                                name="Camera Rotation (degrees)"
                                value={this.currentCameraRotation}
                                onChange={this.onCameraRotationChanged.bind(this)}
                                onClick={this.onCameraRotationClicked.bind(this)}
                                onPointerDown={this.onCameraRotationClicked.bind(this)}
                                selected={this.state.selectedView === 'cameraRotation'}
                            />
                            <TextEntry
                                name="Camera Distance from Screen (cm)"
                                value={this.currentCameraDistanceFromScreen}
                                onChange={this.onCameraDistanceFromScreenChanged.bind(this)}
                                onClick={this.onCameraDistanceFromScreenClicked.bind(this)}
                                onPointerDown={this.onCameraDistanceFromScreenClicked.bind(this)}
                                selected={this.state.selectedView === 'cameraDistanceFromScreen'}
                            />
                        </div>
                    </div>
                    <div className={classes('screen-images', this.state.selectedView)}>
                        <div className={classes('screen-container')}>
                            <div className={classes('screenFrontMock')}>
                                <div className={classes('screenFrontCamera')}>
                                    <div className={classes('screenFrontCameraBottom')}></div>
                                </div>
                                <div className={classes('screenFrontOutside')}></div>
                                <div className={classes('screenFrontInside')}></div>
                                <div className={classes('screenFrontCenterLineVert')}></div>
                                <div className={classes('screenFrontTopLine')}></div>
                                <div className={classes('screenFrontBottomLine')}></div>
                                <p className={classes('screenFrontLabel')}>Front View</p>
                            </div>
                        </div>
                        <div className={classes('screen-container')}>
                            <div className={classes('screenSideMock')}>
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
                                <p className={classes('screenSideLabel')}>Side View</p>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        );
    }
}
