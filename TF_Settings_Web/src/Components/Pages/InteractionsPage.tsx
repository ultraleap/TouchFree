import { Page } from "./Page";
import { RadioGroup } from "../Controls/RadioGroup";
import { RadioLine } from "../Controls/RadioLine";
import { ToggleSwitch } from "../Controls/ToggleSwitch";
import { Slider } from "../Controls/Slider";
import { TextSlider } from "../Controls/TextSlider";

import '../../Styles/Interactions.css';

import AirPushPreview from '../../Videos/AirPush_Preview.webm';
import TouchPlanePreview from '../../Videos/TouchPlane_Preview.webm';
import HoverPreview from '../../Videos/Hover_Preview.webm';

enum Interaction {
    "AirPush" = "AirPush",
    "TouchPlane" = "TouchPlane",
    "Hover & Hold" = "Hover & Hold"
};

interface interactionsState {
    selectedInteraction: number,
    interactionConfig: any
};

export class InteractionsPage extends Page<{}, interactionsState> {
    private videoPaths: string[] = [
        AirPushPreview,
        TouchPlanePreview,
        HoverPreview,
    ];

    constructor(props: {}) {
        super(props);

        let state = {
            // These should come from the current state in the props
            selectedInteraction: 1,
            interactionConfig: {
                responsiveness: 0.04,
                hoverHold: {
                    startTime: 0,
                    completeTime: 1.5
                },
                touchPlane: {
                    planeDistance: 5
                },
                interactionZone: {
                    minDist: 0,
                    maxDist: 30
                }
            },
        };

        this.state = state;
    }

    onInteractionChange(e: React.FormEvent<HTMLInputElement>): void {
        let interaction: Interaction;

        if (!Object.values(Interaction).some((x: string | Interaction) => x.toString() === e.currentTarget.value)) {
            return
        } else {
            interaction = e.currentTarget.value as unknown as Interaction;
            console.log("Selected " + interaction);

            this.setState(() => ({
                selectedInteraction: Object.keys(Interaction).indexOf(interaction),
            }));
        }
    }

    onScrollDragChange(e: React.FormEvent<HTMLInputElement>): void {

    }

    onCursorMovementChange(e: React.FormEvent<HTMLInputElement>): void {
        let newConf = this.state.interactionConfig;

        newConf.responsiveness = parseFloat(e.currentTarget.value);

        this.setState(() => ({
            interactionConfig: newConf
        }));
    }

    onTouchPlaneDistanceChange(e: React.FormEvent<HTMLInputElement>): void {

    }

    onHoverStartTimeChange(e: React.FormEvent<HTMLInputElement>): void {
        let newConf = this.state.interactionConfig;

        newConf.hoverHold.startTime = parseFloat(e.currentTarget.value);

        this.setState(() => ({
            interactionConfig: newConf
        }));
    }

    onHoverCompleteTimeChange(e: React.FormEvent<HTMLInputElement>): void {

    }

    onTrackingPosChange(e: React.FormEvent<HTMLInputElement>): void {

    }

    interactionZoneToggled(e: React.FormEvent<HTMLInputElement>): void {

    }

    onInteractionMinDistChange(e: React.FormEvent<HTMLInputElement>): void {

    }

    onInteractionMaxDistChange(e: React.FormEvent<HTMLInputElement>): void {

    }

    resetToDefaults(): void {

    }

    render() {
        let interactions: string[] = [];
        let trackingOptions: string[] = [
            "Closest Bone to TouchPlane",
            "Index Fingertip"
        ]

        let interactionControls: JSX.Element[] = [];
        let zoneControls: JSX.Element[] = [];

        Object.values(Interaction).forEach((x: string | Interaction) => {
            if (typeof x === "string") {
                interactions.push(x.toString());
            }
        });

        // switch (currentInteractionType){
        interactionControls.push(<ToggleSwitch name="Scroll and Drag"
                                                onChange={this.onScrollDragChange.bind(this)}/>);
        interactionControls.push(<Slider name="Cursor Movement"
                                            rangeMin={.1}
                                            rangeMax={1.5}
                                            leftLabel="Responsive"
                                            rightLabel="Stable"
                                            value={this.state.interactionConfig.responsiveness}
                                            onChange={this.onCursorMovementChange.bind(this)}/>);
        interactionControls.push(<TextSlider name="Hover & Hold Start Time"
                                            rangeMin={.1}
                                            rangeMax={1.5}
                                            leftLabel="0.1s"
                                            rightLabel="1.5s"
                                            value={this.state.interactionConfig.hoverHold.startTime}
                                            onChange={this.onHoverStartTimeChange.bind(this)}/>);
        interactionControls.push(<TextSlider name="Hover & Hold Complete Time"
                                            rangeMin={.1}
                                            rangeMax={1.5}
                                            leftLabel="0.1s"
                                            rightLabel="1.5s"
                                            value={this.state.interactionConfig.hoverHold.completeTime}
                                            onChange={this.onHoverCompleteTimeChange.bind(this)}/>);
        interactionControls.push(<TextSlider name="TouchPlane Distance(cm)"
                                            rangeMin={0}
                                            rangeMax={30}
                                            leftLabel="0cm"
                                            rightLabel="30cm"
                                            value={this.state.interactionConfig.touchPlane.planeDistance}
                                            onChange={this.onTouchPlaneDistanceChange.bind(this)}/>);
        interactionControls.push(<RadioLine name="Tracking Position"
                                            selected={0}
                                            options={trackingOptions}
                                            onChange={this.onTrackingPosChange.bind(this)}/>);

        // }

        // if InteractionZone enabled {
        zoneControls.push(<TextSlider name="Minimum Active Distance"
                                      rangeMin={0}
                                      rangeMax={30}
                                      leftLabel="0cm"
                                      rightLabel="30cm"
                                      value={this.state.interactionConfig.interactionZone.minDist}
                                      onChange={this.onInteractionMinDistChange.bind(this)}/>);
        zoneControls.push(<TextSlider name="Maximum Active Distance"
                                      rangeMin={0}
                                      rangeMax={30}
                                      leftLabel="0cm"
                                      rightLabel="30cm"
                                      value={this.state.interactionConfig.interactionZone.minDist}
                                      onChange={this.onInteractionMaxDistChange.bind(this)}/>);
        // }

        return(
            <div className="page">
                <div className="TitleLine">
                    <h1> Interaction Type </h1>
                    <button
                        onClick={this.resetToDefaults.bind(this)}
                        className="tfButton" >
                        <p> Reset to Default </p>
                    </button>
                </div>

                <div className="horizontalContainer sideSpacing">
                    <RadioGroup
                        name="InteractionType"
                        selected={this.state.selectedInteraction}
                        options={interactions}
                        onChange={this.onInteractionChange.bind(this)} />

                    <video autoPlay loop key={this.state.selectedInteraction} className="InteractionPreview">
                        <source src={this.videoPaths[this.state.selectedInteraction]} type="video/webm"/>
                    </video>
                </div>

                <div className="verticalContainer sideSpacing">
                    {interactionControls}
                </div>

                <div className="TitleLine">
                    <h1> Interaction Zone </h1>
                </div>

                <div className="verticalContainer sideSpacing">
                    <ToggleSwitch name="Enabled"
                                onChange={this.interactionZoneToggled.bind(this)}/>
                    {zoneControls}
                </div>
            </div>
        );
    }
}