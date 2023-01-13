import classnames from 'classnames/bind';

import styles from './Interactions.module.scss';

import { Component } from 'react';

import { ConfigurationManager } from 'TouchFree/src/Configuration/ConfigurationManager';
import { InteractionConfigFull, TrackedPosition } from 'TouchFree/src/Configuration/ConfigurationTypes';
import { ConfigState, WebSocketResponse } from 'TouchFree/src/Connection/TouchFreeServiceTypes';
import { InteractionType } from 'TouchFree/src/TouchFreeToolingTypes';

import { InteractionExplainer } from '@/Videos';

import { LabelledToggleSwitch, RadioGroup, RadioLine, Slider, TextSlider } from '@/Components';

import { DefaultInteractionConfig } from './SettingsTypes';

const classes = classnames.bind(styles);

const InteractionTranslator: Record<string, InteractionType> = {
    AirPush: InteractionType.PUSH,
    Hover: InteractionType.HOVER,
    'Touch Plane': InteractionType.TOUCHPLANE,
};

const TouchPlaneTrackingOptions: Record<string, TrackedPosition> = {
    'Closest Bone to TouchPlane': TrackedPosition.NEAREST,
    'Index Fingertip': TrackedPosition.INDEX_TIP,
};

interface InteractionsState {
    interactionConfig: InteractionConfigFull;
    resetButtonHovered: boolean;
}

export class InteractionsScreen extends Component<{}, InteractionsState> {
    componentDidMount(): void {
        ConfigurationManager.RequestConfigFileState(this.setStateFromFile.bind(this));
    }

    componentDidUpdate(_prevProps: {}, prevState: InteractionsState): void {
        if (!this.state || !prevState) return;
        if (!this.state.interactionConfig || !prevState.interactionConfig) return;

        if (this.state.interactionConfig !== prevState.interactionConfig) {
            ConfigurationManager.RequestConfigFileChange(
                this.state.interactionConfig,
                null,
                this.configChangeCbHandler.bind(this)
            );
        }
    }

    private getStateCopy = (state: InteractionsState): InteractionsState => {
        const stateCopy = { ...state };
        if (!state) return stateCopy;

        stateCopy.interactionConfig = { ...state.interactionConfig };
        stateCopy.interactionConfig.TouchPlane = { ...state.interactionConfig.TouchPlane };
        stateCopy.interactionConfig.HoverAndHold = { ...state.interactionConfig.HoverAndHold };

        return stateCopy;
    };

    configChangeCbHandler(result: WebSocketResponse): void {
        if (result.status !== 'Success') {
            console.error(`Failed to set config state! Info: ${result.message}`);
        }
    }

    setStateFromFile(config: ConfigState): void {
        this.setState((state) => {
            const newState = this.getStateCopy(state);
            newState.interactionConfig = config.interaction;
            return newState;
        });
    }

    // Radio Control Logic
    onInteractionChange(newValue: string): void {
        if (!(newValue in InteractionTranslator)) {
            console.error(`Could not change interaction type; did not recognise the "${newValue}" interaction`);
        }

        const interactionType: InteractionType = InteractionTranslator[newValue];

        this.setState((state) => {
            const newState = this.getStateCopy(state);
            newState.interactionConfig.InteractionType = interactionType;
            return newState;
        });
    }

    // Slider Control Logic
    onCursorMovementChange(newValue: number): void {
        this.setState((state) => {
            const newState = this.getStateCopy(state);
            newState.interactionConfig.DeadzoneRadius = newValue;
            return newState;
        });
    }

    // Toggle Control Logic
    onScrollDragChange(useScroll: boolean): void {
        this.setState((state) => {
            const newState = this.getStateCopy(state);
            newState.interactionConfig.UseScrollingOrDragging = useScroll;
            return newState;
        });
    }

    onTouchPlaneDistanceChange(newValue: number): void {
        this.setState((state) => {
            const newState = this.getStateCopy(state);
            newState.interactionConfig.TouchPlane.TouchPlaneActivationDistanceCm = newValue;
            return newState;
        });
    }

    onTrackingPosChange(newValue: string): void {
        if (!(newValue in TouchPlaneTrackingOptions)) {
            console.error(`Could not Touch Plane tracking target type; did not recognise "${newValue}"`);
        }

        const trackedPos: TrackedPosition = TouchPlaneTrackingOptions[newValue];

        this.setState((state) => {
            const newState = this.getStateCopy(state);
            newState.interactionConfig.TouchPlane.TouchPlaneTrackedPosition = trackedPos;
            return newState;
        });
    }

    onHoverStartTimeChange(newValue: number): void {
        this.setState((state) => {
            const newState = this.getStateCopy(state);
            newState.interactionConfig.HoverAndHold.HoverStartTimeS = newValue;
            return newState;
        });
    }

    onHoverCompleteTimeChange(newValue: number): void {
        this.setState((state) => {
            const newState = this.getStateCopy(state);
            newState.interactionConfig.HoverAndHold.HoverCompleteTimeS = newValue;
            return newState;
        });
    }

    interactionZoneToggled(zoneEnabled: boolean): void {
        this.setState((state) => {
            const newState = this.getStateCopy(state);
            newState.interactionConfig.InteractionZoneEnabled = zoneEnabled;
            return newState;
        });
    }

    onInteractionMinDistChange(newValue: number): void {
        this.setState((state) => {
            const newState = this.getStateCopy(state);
            newState.interactionConfig.InteractionMinDistanceCm = newValue;
            return newState;
        });
    }

    onInteractionMaxDistChange(newValue: number): void {
        this.setState((state) => {
            const newState = this.getStateCopy(state);
            newState.interactionConfig.InteractionMaxDistanceCm = newValue;
            return newState;
        });
    }

    resetToDefaults(): void {
        this.setState((state) => {
            const newState = this.getStateCopy(state);
            newState.interactionConfig = DefaultInteractionConfig;
            return newState;
        });
    }

    onResetEnter = () => {
        this.setState((state) => {
            return { ...state, resetButtonHovered: true };
        });
    };

    onResetLeave = () => {
        this.setState((state) => {
            return { ...state, resetButtonHovered: false };
        });
    };

    render(): JSX.Element {
        let coreBody: JSX.Element = <div />;
        const interactionControls: JSX.Element[] = [];
        const zoneControls: JSX.Element[] = [];

        // TODO: Make it so elements appear even when state is null to prevent flash of empty
        //       screen while reading in config
        if (this.state !== null) {
            const activeInteraction: number = Object.keys(InteractionTranslator).findIndex((key: string) => {
                return InteractionTranslator[key] === this.state.interactionConfig.InteractionType;
            });

            const activePlaneTracking: number = Object.keys(TouchPlaneTrackingOptions).findIndex((key: string) => {
                return (
                    TouchPlaneTrackingOptions[key] === this.state.interactionConfig.TouchPlane.TouchPlaneTrackedPosition
                );
            });

            switch (this.state.interactionConfig.InteractionType) {
                case InteractionType.HOVER:
                    interactionControls.push(
                        <TextSlider
                            name="Hover & Hold Start Time"
                            key="Hover & Hold Start Time"
                            rangeMin={0.1}
                            rangeMax={1}
                            leftLabel="0.1s"
                            rightLabel="1s"
                            value={this.state.interactionConfig.HoverAndHold.HoverStartTimeS}
                            onChange={this.onHoverStartTimeChange.bind(this)}
                        />
                    );
                    interactionControls.push(
                        <TextSlider
                            name="Hover & Hold Complete Time"
                            key="Hover & Hold Complete Time"
                            rangeMin={0.1}
                            rangeMax={1}
                            leftLabel="0.1s"
                            rightLabel="1s"
                            value={this.state.interactionConfig.HoverAndHold.HoverCompleteTimeS}
                            onChange={this.onHoverCompleteTimeChange.bind(this)}
                        />
                    );
                    break;
                case InteractionType.TOUCHPLANE:
                    interactionControls.push(
                        <LabelledToggleSwitch
                            name="Scroll and Drag"
                            key="Scroll and Drag"
                            value={this.state.interactionConfig.UseScrollingOrDragging}
                            onChange={this.onScrollDragChange.bind(this)}
                        />
                    );
                    interactionControls.push(
                        <TextSlider
                            name="TouchPlane Distance(cm)"
                            key="TouchPlane Distance(cm)"
                            rangeMin={0}
                            rangeMax={20}
                            leftLabel="0cm"
                            rightLabel="20cm"
                            value={this.state.interactionConfig.TouchPlane.TouchPlaneActivationDistanceCm}
                            onChange={this.onTouchPlaneDistanceChange.bind(this)}
                        />
                    );
                    interactionControls.push(
                        <RadioLine
                            name="Tracking Position"
                            key="Tracking Position"
                            selected={activePlaneTracking}
                            options={Object.keys(TouchPlaneTrackingOptions)}
                            onChange={this.onTrackingPosChange.bind(this)}
                        />
                    );
                    break;
                case InteractionType.PUSH:
                    interactionControls.push(
                        <LabelledToggleSwitch
                            name="Scroll and Drag"
                            key="Scroll and Drag"
                            value={this.state.interactionConfig.UseScrollingOrDragging}
                            onChange={this.onScrollDragChange.bind(this)}
                        />
                    );
                    break;
            }

            if (this.state.interactionConfig.InteractionZoneEnabled) {
                zoneControls.push(
                    <TextSlider
                        name="Minimum Active Distance"
                        key="Minimum Active Distance"
                        rangeMin={0}
                        rangeMax={30}
                        leftLabel="0cm"
                        rightLabel="30cm"
                        value={this.state.interactionConfig.InteractionMinDistanceCm}
                        onChange={this.onInteractionMinDistChange.bind(this)}
                    />
                );
                zoneControls.push(
                    <TextSlider
                        name="Maximum Active Distance"
                        key="Maximum Active Distance"
                        rangeMin={0}
                        rangeMax={30}
                        leftLabel="0cm"
                        rightLabel="30cm"
                        value={this.state.interactionConfig.InteractionMaxDistanceCm}
                        onChange={this.onInteractionMaxDistChange.bind(this)}
                    />
                );
            }

            coreBody = (
                <>
                    <div className={classes('title-line')}>
                        <h1> Interaction Type </h1>
                        <button
                            onClick={this.resetToDefaults.bind(this)}
                            onPointerUp={this.resetToDefaults.bind(this)}
                            onPointerEnter={this.onResetEnter}
                            onPointerLeave={this.onResetLeave}
                            className={
                                this.state?.resetButtonHovered
                                    ? `${classes('reset-button')} hover`
                                    : classes('reset-button')
                            }
                        >
                            <p> Reset to Default </p>
                        </button>
                    </div>
                    <div className={classes('section-container')}>
                        <div className={classes('content')}>
                            <div className={classes('horizontalContainer')}>
                                <RadioGroup
                                    name="InteractionType"
                                    selected={activeInteraction}
                                    options={Object.keys(InteractionTranslator)}
                                    onChange={this.onInteractionChange.bind(this)}
                                />
                                <video
                                    autoPlay={true}
                                    loop={true}
                                    key={InteractionExplainer}
                                    className={classes(
                                        'InteractionPreview',
                                        'Interaction',
                                        activeInteraction.toString()
                                    )}
                                >
                                    <source src={InteractionExplainer} />
                                </video>
                            </div>
                            <div className={classes('verticalContainer')}>
                                <Slider
                                    name="Cursor Movement"
                                    increment={0.0001}
                                    rangeMin={0}
                                    rangeMax={0.015}
                                    leftLabel="Responsive"
                                    rightLabel="Stable"
                                    value={this.state.interactionConfig.DeadzoneRadius}
                                    onChange={this.onCursorMovementChange.bind(this)}
                                />
                            </div>
                        </div>
                        <div className={classes('content')}>
                            <div className={classes('verticalContainer')}>{interactionControls}</div>
                        </div>
                    </div>
                    <h1 className={classes('title-line')}> Interaction Zone </h1>
                    <div className={classes('section-container')}>
                        <div className={classes('content')}>
                            <div className={classes('verticalContainer')}>
                                <LabelledToggleSwitch
                                    name="Enable/Disable"
                                    value={this.state.interactionConfig.InteractionZoneEnabled}
                                    onChange={this.interactionZoneToggled.bind(this)}
                                />
                                {zoneControls}
                            </div>
                        </div>
                    </div>
                </>
            );
        }

        return <div className={classes('container')}>{coreBody}</div>;
    }
}
