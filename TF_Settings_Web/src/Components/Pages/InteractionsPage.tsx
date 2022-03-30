import { InteractionConfigFull, TrackedPosition } from "../../TouchFree/Configuration/ConfigurationTypes";
import { ConfigurationManager } from "../../TouchFree/Configuration/ConfigurationManager";
import { ConfigState, WebSocketResponse } from "../../TouchFree/Connection/TouchFreeServiceTypes";
import { InteractionType } from "../../TouchFree/TouchFreeToolingTypes";

import { Page } from "./Page";
import { RadioGroup } from "../Controls/RadioGroup";
import { RadioLine } from "../Controls/RadioLine";
import { ToggleSwitch } from "../Controls/ToggleSwitch";
import { Slider } from "../Controls/Slider";
import { TextSlider } from "../Controls/TextSlider";
import { DefaultInteractionConfig } from "../SettingsTypes";

import '../../Styles/Interactions.css';

import AirPushPreview from '../../Videos/AirPush_Preview.webm';
import TouchPlanePreview from '../../Videos/TouchPlane_Preview.webm';
import HoverPreview from '../../Videos/Hover_Preview.webm';

const InteractionTranslator: Record<string, InteractionType> = {
    "AirPush": InteractionType.PUSH,
    "Hover": InteractionType.HOVER,
    "Touch Plane": InteractionType.TOUCHPLANE,
};

const TouchPlaneTrackingOptions: Record<string, TrackedPosition> = {
    "Closest Bone to TouchPlane": TrackedPosition.NEAREST,
    "Index Fingertip": TrackedPosition.INDEX_TIP
};

interface interactionsState {
    interactionConfig: InteractionConfigFull
};

export class InteractionsPage extends Page<{}, interactionsState> {
    private videoPaths: string[] = [
        AirPushPreview,
        HoverPreview,
        TouchPlanePreview,
    ];

    componentDidMount(): void {
        ConfigurationManager.RequestConfigFileState(this.setStateFromFile.bind(this));
    }

    componentDidUpdate(prevProps: {}, prevState: interactionsState): void {
        if (this.state !== null &&
            this.state !== prevState) {
            ConfigurationManager.RequestConfigFileChange(this.state.interactionConfig, null, this.configChangeCbHandler.bind(this));
        }
    }

    configChangeCbHandler(result: WebSocketResponse): void {
        if (result.status !== "Success") {
            console.error(`Failed to set config state! Info: ${result.message}`);
        }
    }

    setStateFromFile(config: ConfigState): void {
        this.setState({
            interactionConfig: config.interaction
        });
    }

    // Radio Control Logic
    onInteractionChange(e: React.FormEvent<HTMLInputElement>): void {
        if (!(e.currentTarget.value in InteractionTranslator)) {
            console.error(`Could not change interaction type; did not recognise the "${e.currentTarget.value}" interaction`);
        }

        let interactionType: InteractionType = InteractionTranslator[e.currentTarget.value];

        this.setState((state) => {
            let newConf: InteractionConfigFull = {
                ...state.interactionConfig,
                InteractionType: interactionType
            };

            return {
                interactionConfig: newConf
            }
        });
    }

    onTrackingPosChange(e: React.FormEvent<HTMLInputElement>): void {
        if (!(e.currentTarget.value in TouchPlaneTrackingOptions)) {
            console.error(`Could not Touch Plane tracking target type; did not recognise "${e.currentTarget.value}"`);
        }

        let trackedPos: TrackedPosition = TouchPlaneTrackingOptions[e.currentTarget.value];

        this.setState((state) => {
            let newConf: InteractionConfigFull = state.interactionConfig;

            newConf.TouchPlane.TouchPlaneTrackedPosition = trackedPos

            return {
                interactionConfig: newConf
            }
        });
    }

    // Toggle Control Logic
    onScrollDragChange(e: boolean): void {
        // console.log(`Got scrolldrag change to ${e}`);

        let useScroll: boolean = e; //.currentTarget.checked;

        this.setState((state) => {
            let newConf: InteractionConfigFull = {
                ...state.interactionConfig,
                UseScrollingOrDragging: useScroll
            };

            return {
                interactionConfig: newConf
            }
        });
    }

    interactionZoneToggled(e: boolean): void {
        let zoneEnabled: boolean = e; //.currentTarget.checked;

        this.setState((state) => {
            let newConf: InteractionConfigFull = {
                ...state.interactionConfig,
                InteractionZoneEnabled: zoneEnabled
            };

            return {
                interactionConfig: newConf
            }
        });
    }

    // Slider Control Logic
    onCursorMovementChange(e: React.FormEvent<HTMLInputElement>): void {
        let newRadius: number = parseFloat(e.currentTarget.value);

        this.setState((state) => {
            let newConf: InteractionConfigFull = {
                ...state.interactionConfig,
                DeadzoneRadius: newRadius
            };

            return {
                interactionConfig: newConf
            }
        });
    }

    onTouchPlaneDistanceChange(e: React.FormEvent<HTMLInputElement>): void {
        let activationDistance: number = parseFloat(e.currentTarget.value);

        this.setState((state) => {
            let newConf = state.interactionConfig;

            newConf.TouchPlane.TouchPlaneActivationDistanceCM = activationDistance;

            return {
                interactionConfig: newConf
            }
        });
    }

    onHoverStartTimeChange(e: React.FormEvent<HTMLInputElement>): void {
        let hoverStartTime: number = parseFloat(e.currentTarget.value);

        this.setState((state) => {
            let newConf = state.interactionConfig;

            newConf.HoverAndHold.HoverStartTimeS = hoverStartTime;

            return {
                interactionConfig: newConf
            }
        });
    }

    onHoverCompleteTimeChange(e: React.FormEvent<HTMLInputElement>): void {
        let hoverEndTime: number = parseFloat(e.currentTarget.value);

        this.setState((state) => {
            let newConf = state.interactionConfig;

            newConf.HoverAndHold.HoverCompleteTimeS = hoverEndTime;

            return {
                interactionConfig: newConf
            }
        });
    }

    onInteractionMinDistChange(e: React.FormEvent<HTMLInputElement>): void {
        let interactionMinDist: number = parseFloat(e.currentTarget.value);

        this.setState((state) => {
            let newConf: InteractionConfigFull = {
                ...state.interactionConfig,
                InteractionMinDistanceCm: interactionMinDist
            };

            return {
                interactionConfig: newConf
            }
        });
    }

    onInteractionMaxDistChange(e: React.FormEvent<HTMLInputElement>): void {
        let interactionMaxDist: number = parseFloat(e.currentTarget.value);

        this.setState((state) => {
            let newConf: InteractionConfigFull =
            {
                ...state.interactionConfig,
                InteractionMaxDistanceCm: interactionMaxDist
            };

            return {
                interactionConfig: newConf
            }
        });
    }

    resetToDefaults(): void {
        this.setState({
            interactionConfig: DefaultInteractionConfig
        });
    }

    render(): JSX.Element {
        let coreBody: JSX.Element = <div></div>;
        let interactionControls: JSX.Element[] = [];
        let zoneControls: JSX.Element[] = [];

        if (this.state !== null) {
            let activeInteraction: number = Object.keys(InteractionTranslator).findIndex((key: string) => {
                return InteractionTranslator[key] === this.state.interactionConfig.InteractionType;
            });

            let activePlaneTracking: number = Object.keys(TouchPlaneTrackingOptions).findIndex((key: string) => {
                return TouchPlaneTrackingOptions[key] === this.state.interactionConfig.TouchPlane.TouchPlaneTrackedPosition;
            });

            switch (this.state.interactionConfig.InteractionType) {
                case InteractionType.HOVER:
                    interactionControls.push(<TextSlider name="Hover & Hold Start Time"
                        rangeMin={.1}
                        rangeMax={1}
                        leftLabel="0.1s"
                        rightLabel="1s"
                        value={this.state.interactionConfig.HoverAndHold.HoverStartTimeS}
                        onChange={this.onHoverStartTimeChange.bind(this)} />);
                    interactionControls.push(<TextSlider name="Hover & Hold Complete Time"
                        rangeMin={.1}
                        rangeMax={1}
                        leftLabel="0.1s"
                        rightLabel="1s"
                        value={this.state.interactionConfig.HoverAndHold.HoverCompleteTimeS}
                        onChange={this.onHoverCompleteTimeChange.bind(this)} />);
                    break;
                case InteractionType.TOUCHPLANE:
                    interactionControls.push(<ToggleSwitch name="Scroll and Drag"
                        value={this.state.interactionConfig.UseScrollingOrDragging}
                        onChange={this.onScrollDragChange.bind(this)} />);
                    interactionControls.push(<TextSlider name="TouchPlane Distance(cm)"
                        rangeMin={0}
                        rangeMax={20}
                        leftLabel="0cm"
                        rightLabel="20cm"
                        value={this.state.interactionConfig.TouchPlane.TouchPlaneActivationDistanceCM}
                        onChange={this.onTouchPlaneDistanceChange.bind(this)} />);
                    interactionControls.push(<RadioLine name="Tracking Position"
                        selected={activePlaneTracking}
                        options={Object.keys(TouchPlaneTrackingOptions)}
                        onChange={this.onTrackingPosChange.bind(this)} />);
                    break;
                case InteractionType.PUSH:
                    interactionControls.push(<ToggleSwitch name="Scroll and Drag"
                        value={this.state.interactionConfig.UseScrollingOrDragging}
                        onChange={this.onScrollDragChange.bind(this)} />);
                    break;
            }

            if (this.state.interactionConfig.InteractionZoneEnabled) {
                zoneControls.push(<TextSlider name="Minimum Active Distance"
                    rangeMin={0}
                    rangeMax={30}
                    leftLabel="0cm"
                    rightLabel="30cm"
                    value={this.state.interactionConfig.InteractionMinDistanceCm}
                    onChange={this.onInteractionMinDistChange.bind(this)} />);
                zoneControls.push(<TextSlider name="Maximum Active Distance"
                    rangeMin={0}
                    rangeMax={30}
                    leftLabel="0cm"
                    rightLabel="30cm"
                    value={this.state.interactionConfig.InteractionMaxDistanceCm}
                    onChange={this.onInteractionMaxDistChange.bind(this)} />);
            }

            coreBody =
                <div>

                    <div className="horizontalContainer sideSpacing">
                        <RadioGroup
                            name="InteractionType"
                            selected={activeInteraction}
                            options={Object.keys(InteractionTranslator)}
                            onChange={this.onInteractionChange.bind(this)} />

                        <video autoPlay loop key={this.state.interactionConfig.InteractionType} className="InteractionPreview">
                            <source src={this.videoPaths[activeInteraction]} type="video/webm" />
                        </video>
                    </div>

                    <div className="verticalContainer sideSpacing">
                        <Slider name="Cursor Movement"
                            increment={0.001}
                            rangeMin={0}
                            rangeMax={1}
                            leftLabel="Responsive"
                            rightLabel="Stable"
                            value={this.state.interactionConfig.DeadzoneRadius}
                            onChange={this.onCursorMovementChange.bind(this)} />
                        {interactionControls}
                    </div>

                    <div className="TitleLine">
                        <h1> Interaction Zone </h1>
                    </div>

                    <div className="verticalContainer sideSpacing">
                        <ToggleSwitch name="Enable/Disable"
                            value={this.state.interactionConfig.InteractionZoneEnabled}
                            onChange={this.interactionZoneToggled.bind(this)} />
                        {zoneControls}
                    </div>
                </div>;
        }

        return (
            <div className="page">
                <div className="TitleLine">
                    <h1> Interaction Type </h1>
                    <button
                        onClick={this.resetToDefaults.bind(this)}
                        className="tfButton" >
                        <p> Reset to Default </p>
                    </button>
                </div>

                {coreBody}
            </div>
        );
    }
}