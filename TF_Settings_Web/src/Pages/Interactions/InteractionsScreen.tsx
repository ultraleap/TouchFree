import classnames from 'classnames/bind';

import styles from './Interactions.module.scss';

import { useEffect, useReducer, useState, CSSProperties } from 'react';

import { ConfigurationManager } from 'TouchFree/src/Configuration/ConfigurationManager';
import {
    InteractionConfig,
    InteractionConfigFull,
    TrackedPosition,
} from 'TouchFree/src/Configuration/ConfigurationTypes';
import { InteractionType } from 'TouchFree/src/TouchFreeToolingTypes';

import { InteractionExplainer } from '@/Videos';

import { DocsLink, LabelledToggleSwitch, RadioGroup, RadioLine, Slider, TextSlider } from '@/Components';
import { MiscTextButton } from '@/Components/TFButton/TFButtons';

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

type ReducerAction = { type: 'reset' } | { type: 'update'; content: Partial<InteractionConfig> };

const reducer = (state: InteractionConfig, action: ReducerAction) => {
    switch (action.type) {
        case 'reset':
            return DefaultInteractionConfig;
        case 'update': {
            const newState = { ...state, ...action.content };
            ConfigurationManager.RequestConfigFileChange(newState, null, (result) => {
                if (result.status !== 'Success') {
                    console.error(`Failed to set config state! Info: ${result.message}`);
                }
            });
            return newState as InteractionConfigFull;
        }
    }
};

const InteractionsScreen = () => {
    const [config, dispatch] = useReducer(reducer, DefaultInteractionConfig);
    const [activeInteraction, setActiveInteraction] = useState(0);
    const [activePlaneTracking, setActivePlaneTracking] = useState(0);
    const [supportPosition, setSupportPosition] = useState<CSSProperties | undefined>();

    useEffect(() => {
        ConfigurationManager.RequestConfigFileState((config) =>
            dispatch({ type: 'update', content: config.interaction })
        );

        const onResize = () => {
            setSupportPosition(
                innerHeight > innerWidth ? { position: 'fixed', bottom: '2vh', right: '2vh' } : undefined
            );
        };

        window.addEventListener('resize', onResize);

        return () => window.removeEventListener('resize', onResize);
    }, []);

    useEffect(() => {
        setActiveInteraction(
            Object.keys(InteractionTranslator).findIndex(
                (key: string) => InteractionTranslator[key] === config.InteractionType
            )
        );
    }, [config.InteractionType]);

    useEffect(() => {
        setActivePlaneTracking(
            Object.keys(TouchPlaneTrackingOptions).findIndex(
                (key: string) => TouchPlaneTrackingOptions[key] === config.TouchPlane.TouchPlaneTrackedPosition
            )
        );
    }, [config.TouchPlane.TouchPlaneTrackedPosition]);

    const getInteractionControls = () => {
        switch (config.InteractionType) {
            case InteractionType.HOVER:
                return (
                    <>
                        <TextSlider
                            name="Hover & Hold Start Time"
                            key="Hover & Hold Start Time"
                            rangeMin={0.1}
                            rangeMax={1}
                            leftLabel="0.1s"
                            rightLabel="1s"
                            value={config.HoverAndHold.HoverStartTimeS}
                            onChange={(value) =>
                                dispatch({ type: 'update', content: { HoverAndHold: { HoverStartTimeS: value } } })
                            }
                        />
                        <TextSlider
                            name="Hover & Hold Complete Time"
                            key="Hover & Hold Complete Time"
                            rangeMin={0.1}
                            rangeMax={1}
                            leftLabel="0.1s"
                            rightLabel="1s"
                            value={config.HoverAndHold.HoverCompleteTimeS}
                            onChange={(value) =>
                                dispatch({ type: 'update', content: { HoverAndHold: { HoverCompleteTimeS: value } } })
                            }
                        />
                    </>
                );
            case InteractionType.TOUCHPLANE:
                return (
                    <>
                        <LabelledToggleSwitch
                            name="Scroll and Drag"
                            key="Scroll and Drag"
                            value={config.UseScrollingOrDragging}
                            onChange={(value) =>
                                dispatch({ type: 'update', content: { UseScrollingOrDragging: value } })
                            }
                        />
                        <TextSlider
                            name="TouchPlane Distance(cm)"
                            key="TouchPlane Distance(cm)"
                            rangeMin={0}
                            rangeMax={20}
                            leftLabel="0cm"
                            rightLabel="20cm"
                            value={config.TouchPlane.TouchPlaneActivationDistanceCm}
                            onChange={(value) =>
                                dispatch({
                                    type: 'update',
                                    content: { TouchPlane: { TouchPlaneActivationDistanceCm: value } },
                                })
                            }
                        />
                        <RadioLine
                            name="Tracking Position"
                            key="Tracking Position"
                            selected={activePlaneTracking}
                            options={Object.keys(TouchPlaneTrackingOptions)}
                            onChange={(value) =>
                                dispatch({
                                    type: 'update',
                                    content: {
                                        TouchPlane: { TouchPlaneTrackedPosition: TouchPlaneTrackingOptions[value] },
                                    },
                                })
                            }
                        />
                    </>
                );
            case InteractionType.PUSH:
                return (
                    <>
                        <LabelledToggleSwitch
                            name="Scroll and Drag"
                            key="Scroll and Drag"
                            value={config.UseScrollingOrDragging}
                            onChange={(value) =>
                                dispatch({ type: 'update', content: { UseScrollingOrDragging: value } })
                            }
                        />
                    </>
                );
        }
    };

    return (
        <div className={classes('container')}>
            <div className={classes('title-line')}>
                <h1> Interaction Type </h1>
                <div className={classes('misc-button-container')}>
                    <DocsLink
                        title="Support"
                        url="https://www.ultraleap.com/contact-us/"
                        buttonStyle={supportPosition}
                    />
                    <MiscTextButton title="Reset to Default" onClick={() => dispatch({ type: 'reset' })} />
                </div>
            </div>
            <div className={classes('section-container')}>
                <div className={classes('content')}>
                    <div className={classes('horizontalContainer')}>
                        <RadioGroup
                            name="InteractionType"
                            selected={activeInteraction}
                            options={Object.keys(InteractionTranslator)}
                            onChange={(value) =>
                                dispatch({
                                    type: 'update',
                                    content: { InteractionType: InteractionTranslator[value] },
                                })
                            }
                        />
                        <video
                            autoPlay={true}
                            loop={true}
                            key={InteractionExplainer}
                            className={classes('InteractionPreview', `Interaction${activeInteraction.toString()}`)}
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
                            value={config.DeadzoneRadius}
                            onChange={(value) => dispatch({ type: 'update', content: { DeadzoneRadius: value } })}
                        />
                    </div>
                </div>
                <div className={classes('content')}>
                    <div className={classes('verticalContainer')}>{getInteractionControls()}</div>
                </div>
            </div>
            <h1 className={classes('title-line')}> Interaction Zone </h1>
            <div className={classes('section-container')}>
                <div className={classes('content')}>
                    <div className={classes('verticalContainer')}>
                        <LabelledToggleSwitch
                            name="Enable/Disable"
                            value={config.InteractionZoneEnabled}
                            onChange={(value) =>
                                dispatch({ type: 'update', content: { InteractionZoneEnabled: value } })
                            }
                        />
                    </div>
                </div>
                <div className={classes('content')}>
                    <div className={classes('verticalContainer')}>
                        {config.InteractionZoneEnabled ? (
                            <>
                                <TextSlider
                                    name="Minimum Active Distance"
                                    key="Minimum Active Distance"
                                    rangeMin={0}
                                    rangeMax={30}
                                    leftLabel="0cm"
                                    rightLabel="30cm"
                                    value={config.InteractionMinDistanceCm}
                                    onChange={(value) =>
                                        dispatch({ type: 'update', content: { InteractionMinDistanceCm: value } })
                                    }
                                />
                                <TextSlider
                                    name="Maximum Active Distance"
                                    key="Maximum Active Distance"
                                    rangeMin={0}
                                    rangeMax={30}
                                    leftLabel="0cm"
                                    rightLabel="30cm"
                                    value={config.InteractionMaxDistanceCm}
                                    onChange={(value) =>
                                        dispatch({ type: 'update', content: { InteractionMaxDistanceCm: value } })
                                    }
                                />
                            </>
                        ) : (
                            <></>
                        )}
                    </div>
                </div>
            </div>
        </div>
    );
};

export default InteractionsScreen;
