import { TextEntry } from "../Controls/TextEntry";

import '../../Styles/CameraPage.css';

import { Page } from "./Page";
import { PhysicalConfig } from "../../TouchFree/Configuration/ConfigurationTypes";
import { ConfigurationManager } from "../../TouchFree/Configuration/ConfigurationManager";
import { ConfigState, WebSocketResponse } from "../../TouchFree/Connection/TouchFreeServiceTypes";
import { ConnectionManager } from "../../TouchFree/Connection/ConnectionManager";

interface physicalState {
    screenHeight: number;
    cameraHeight: number;
    cameraLeftToRight: number;
    screenTilt: number;
    cameraRotation: number;
    cameraDistanceFromScreen: number;
    physicalConfig: PhysicalConfig;
    selectedView: string;
};

export class CameraPage extends Page<{}, physicalState> {
    constructor(props: {}) {
        super(props);

        let state: physicalState = {
            screenHeight: 0,
            cameraHeight: 0,
            cameraLeftToRight: 0,
            screenTilt: 0,
            cameraRotation: 0,
            cameraDistanceFromScreen: 0,
            physicalConfig: { LeapPositionRelativeToScreenBottomM: { X: 0, Y: 0, Z: 0 }, LeapRotationD: { X: 0, Y: 0, Z: 0 }, ScreenHeightM: 0, ScreenRotationD: 0 },
            selectedView: "screenHeight"
        };

        this.state = state;
    }

    componentDidMount(): void {
        ConnectionManager.AddConnectionListener(() => {
            ConfigurationManager.RequestConfigFileState(this.setStateFromFile.bind(this));
        });
    }

    setStateFromFile(config: ConfigState): void {
        this.setState({
            screenHeight: this.roundToFiveDecimals(config.physical.ScreenHeightM * 100),
            cameraHeight: this.roundToFiveDecimals(config.physical.LeapPositionRelativeToScreenBottomM.Y * 100),
            cameraLeftToRight: this.roundToFiveDecimals(config.physical.LeapPositionRelativeToScreenBottomM.X * 100),
            cameraDistanceFromScreen: this.roundToFiveDecimals(-config.physical.LeapPositionRelativeToScreenBottomM.Z * 100),
            cameraRotation: config.physical.LeapRotationD.X,
            physicalConfig: config.physical,
            screenTilt: config.physical.ScreenRotationD
        });
    }

    roundToFiveDecimals(numberIn: number): number {
        return Math.round(numberIn * 100000) / 100000;
    }

    componentDidUpdate(prevProps: {}, prevState: physicalState): void {
        if (this.state !== null &&
            this.state !== prevState) {
            let physicalConfigState: PhysicalConfig = {
                ...this.state.physicalConfig,
                LeapPositionRelativeToScreenBottomM: {
                    X: this.state.cameraLeftToRight / 100,
                    Y: this.state.cameraHeight / 100,
                    Z: -this.state.cameraDistanceFromScreen / 100
                },
                LeapRotationD: {
                    ...this.state.physicalConfig.LeapRotationD,
                    X: this.state.cameraRotation
                },
                ScreenHeightM: this.state.screenHeight / 100,
                ScreenRotationD: this.state.screenTilt
            }
            ConfigurationManager.RequestConfigFileChange(null, physicalConfigState, this.configChangeCbHandler.bind(this));
        }
    }

    configChangeCbHandler(result: WebSocketResponse): void {
        if (result.status !== "Success") {
            console.error(`Failed to set config state! Info: ${result.message}`);
        }
    }

    onScreenHeightChanged(e: React.FormEvent<HTMLInputElement>): void {
        const value = parseFloat(e.currentTarget?.value) ?? 0;
        this.setState({ screenHeight: value });
    }

    onCameraHeightChanged(e: React.FormEvent<HTMLInputElement>): void {
        const value = parseFloat(e.currentTarget?.value) ?? 0;
        this.setState({ cameraHeight: value });
    }

    onCameraLeftToRightChanged(e: React.FormEvent<HTMLInputElement>): void {
        const value = parseFloat(e.currentTarget?.value) ?? 0;
        this.setState({ cameraLeftToRight: value });
    }

    onScreenTiltChanged(e: React.FormEvent<HTMLInputElement>): void {
        const value = parseFloat(e.currentTarget?.value) ?? 0;
        this.setState({ screenTilt: value });
    }

    onCameraRotationChanged(e: React.FormEvent<HTMLInputElement>): void {
        const value = parseFloat(e.currentTarget?.value) ?? 0;
        this.setState({ cameraRotation: value });
    }

    onCameraDistanceFromScreenChanged(e: React.FormEvent<HTMLInputElement>): void {
        const value = parseFloat(e.currentTarget?.value) ?? 0;
        this.setState({ cameraDistanceFromScreen: value });
    }

    onScreenHeightClicked(): void {
        this.setState({ selectedView: "screenHeight" });
    }

    onCameraHeightClicked(): void {
        this.setState({ selectedView: "cameraHeight" });
    }

    onCameraLeftToRightClicked(): void {
        this.setState({ selectedView: "cameraLeftToRight" });
    }

    onScreenTiltClicked(): void {
        this.setState({ selectedView: "screenTilt" });
    }

    onCameraRotationClicked(): void {
        this.setState({ selectedView: "cameraRotation" });
    }

    onCameraDistanceFromScreenClicked(): void {
        this.setState({ selectedView: "cameraDistanceFromScreen" });
    }

    render() {
        return (
            <div className={"page " + this.state.selectedView}>
                <div className="TitleLine">
                    <h1> Manual Calibration </h1>
                </div>

                <div className="horizontalContainer sideSpacing">
                    <div className="verticalContainer">
                        <TextEntry name="Screen Height (cm)"
                            value={this.state.screenHeight}
                            onChange={this.onScreenHeightChanged.bind(this)}
                            onClick={this.onScreenHeightClicked.bind(this)}
                            onPointerDown={this.onScreenHeightClicked.bind(this)}
                            selected={this.state.selectedView === "screenHeight"} />
                        <TextEntry name="Camera Height (cm)"
                            value={this.state.cameraHeight}
                            onChange={this.onCameraHeightChanged.bind(this)}
                            onClick={this.onCameraHeightClicked.bind(this)}
                            onPointerDown={this.onCameraHeightClicked.bind(this)}
                            selected={this.state.selectedView === "cameraHeight"} />
                        <TextEntry name="Camera Left to Right (cm)"
                            value={this.state.cameraLeftToRight}
                            onChange={this.onCameraLeftToRightChanged.bind(this)}
                            onClick={this.onCameraLeftToRightClicked.bind(this)}
                            onPointerDown={this.onCameraLeftToRightClicked.bind(this)}
                            selected={this.state.selectedView === "cameraLeftToRight"} />
                        <div className="cameraPageDivider"></div>
                        <TextEntry name="Screen Tilt (degrees)"
                            value={this.state.screenTilt}
                            onChange={this.onScreenTiltChanged.bind(this)}
                            onClick={this.onScreenTiltClicked.bind(this)}
                            onPointerDown={this.onScreenTiltClicked.bind(this)}
                            selected={this.state.selectedView === "screenTilt"} />
                        <TextEntry name="Camera Rotation (degrees)"
                            value={this.state.cameraRotation}
                            onChange={this.onCameraRotationChanged.bind(this)}
                            onClick={this.onCameraRotationClicked.bind(this)}
                            onPointerDown={this.onCameraRotationClicked.bind(this)}
                            selected={this.state.selectedView === "cameraRotation"} />
                        <TextEntry name="Camera Distance from Screen (cm)"
                            value={this.state.cameraDistanceFromScreen}
                            onChange={this.onCameraDistanceFromScreenChanged.bind(this)}
                            onClick={this.onCameraDistanceFromScreenClicked.bind(this)}
                            onPointerDown={this.onCameraDistanceFromScreenClicked.bind(this)}
                            selected={this.state.selectedView === "cameraDistanceFromScreen"} />
                    </div>

                    <div className="verticalContainer">
                        <div className="screenFrontWrapper">
                            <div className="screenFrontMock">
                                <div className="screenFrontCamera">
                                    <div className="screenFrontCameraBottom">
                                    </div>
                                </div>
                                <div className="screenFrontOutside"></div>
                                <div className="screenFrontInside"></div>
                                <div className="screenFrontCenterLineVert"></div>
                                <div className="screenFrontTopLine"></div>
                                <div className="screenFrontBottomLine"></div>
                                <p className="screenFrontLabel">Front View</p>
                            </div>
                        </div>

                        <div className="screenSideWrapper">
                            <div className="screenSideMock">
                                <div className="screenSideCamera">
                                    <div className="screenSideCameraBottom">
                                    </div>
                                </div>
                                <div className="screenSideOutside"></div>
                                <div className="screenSideInside"></div>
                                <div className="screenSideCenterLineVert"></div>
                                <div className="screenSideExtraLineVert"></div>
                                <div className="screenSideTopLine"></div>
                                <div className="screenSideBottomLine"></div>
                                <div className="screenSideTiltBox"></div>
                                <p className="screenSideLabel">Side View</p>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        );
    }
}