import { CSSProperties } from "react";
import { TextEntry } from "../Controls/TextEntry";

import '../../Styles/CameraPage.css';

import { Page } from "./Page";

interface phyiscalState {
    physicalConfig: any;
    selectedView: string;
};

export class CameraPage extends Page<{}, phyiscalState> {
    private divStyle: CSSProperties = {
        flex: '1 1 auto',
        width: '100%',
        height: '800px',
        backgroundColor: 'red',
    };

    constructor(props: {}) {
        super(props);

        let state = {
            // These should come from the current state in the props
            physicalConfig: {
                screenHeightM: 0.04,
                leapPositionRelativeToScreenBottomM: {
                    x: 0.00038,
                    y: -0.2213,
                    z: -0.1442
                },
                leapRotationD: {
                    x: -3.5,
                    y: 0.0,
                    z: 0.0
                },
                screenRotationD: 0.0,
                screenWidthPX: 507,
                screenHeightPX: 285
            },
            selectedView: "screenHeight"
        };

        this.state = state;
    }

    onScreenHeightChanged(e: React.FormEvent<HTMLInputElement>): void {
    }
    onCameraHeightChanged(e: React.FormEvent<HTMLInputElement>): void {
    }
    onCameraLeftToRightChanged(e: React.FormEvent<HTMLInputElement>): void {
    }

    onScreenHeightClicked(): void {
        this.setState({...this.state, selectedView: "screenHeight"});
    }

    onCameraHeightClicked(): void {
        this.setState({...this.state, selectedView: "cameraHeight"});
    }

    onCameraLeftToRightClicked(): void {
        this.setState({...this.state, selectedView: "cameraLeftToRight"});
    }

    onScreenTiltClicked(): void {
        this.setState({...this.state, selectedView: "screenTilt"});
    }

    onCameraRotationClicked(): void {
        this.setState({...this.state, selectedView: "cameraRotation"});
    }

    onCameraDistanceFromScreenClicked(): void {
        this.setState({...this.state, selectedView: "cameraDistanceFromScreen"});
    }

    render() {
        return(
            <div className={"page " + this.state.selectedView}>
                <div className="TitleLine">
                    <h1> Manual Calibration </h1>
                </div>

                <div className="horizontalContainer sideSpacing">
                    <div className="verticalContainer">
                        <TextEntry name="Screen Height (cm)"
                                    value={this.state.physicalConfig.screenHeightM}
                                    onChange={this.onScreenHeightChanged.bind(this)}
                                    onClick={this.onScreenHeightClicked.bind(this)}
                                    onPointerDown={this.onScreenHeightClicked.bind(this)}
                                    selected={this.state.selectedView === "screenHeight"}/>
                        <TextEntry name="Camera Height (cm)"
                                    value={this.state.physicalConfig.leapPositionRelativeToScreenBottomM.y}
                                    onChange={this.onCameraHeightChanged.bind(this)}
                                    onClick={this.onCameraHeightClicked.bind(this)}
                                    onPointerDown={this.onCameraHeightClicked.bind(this)}
                                    selected={this.state.selectedView === "cameraHeight"}/>
                        <TextEntry name="Camera Left to Right (cm)"
                                    value={this.state.physicalConfig.leapPositionRelativeToScreenBottomM.x}
                                    onChange={this.onCameraLeftToRightChanged.bind(this)}
                                    onClick={this.onCameraLeftToRightClicked.bind(this)}
                                    onPointerDown={this.onCameraLeftToRightClicked.bind(this)}
                                    selected={this.state.selectedView === "cameraLeftToRight"}/>
                                    
                        <TextEntry name="Screen Tilt (degrees)"
                                    value={this.state.physicalConfig.screenRotationD}
                                    onChange={this.onScreenHeightChanged.bind(this)}
                                    onClick={this.onScreenTiltClicked.bind(this)}
                                    onPointerDown={this.onScreenTiltClicked.bind(this)}
                                    selected={this.state.selectedView === "screenTilt"}/>
                        <TextEntry name="Camera Rotation (degrees)"
                                    value={this.state.physicalConfig.leapRotationD.x}
                                    onChange={this.onCameraHeightChanged.bind(this)}
                                    onClick={this.onCameraRotationClicked.bind(this)}
                                    onPointerDown={this.onCameraRotationClicked.bind(this)}
                                    selected={this.state.selectedView === "cameraRotation"}/>
                        <TextEntry name="Camera Distance from Screen (cm)"
                                    value={this.state.physicalConfig.leapPositionRelativeToScreenBottomM.z}
                                    onChange={this.onCameraLeftToRightChanged.bind(this)}
                                    onClick={this.onCameraDistanceFromScreenClicked.bind(this)}
                                    onPointerDown={this.onCameraDistanceFromScreenClicked.bind(this)}
                                    selected={this.state.selectedView === "cameraDistanceFromScreen"}/>
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
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        );
    }
}