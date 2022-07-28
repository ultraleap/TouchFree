import { v4 as uuidgen } from 'uuid';

import 'Styles/Camera/CameraMasking.scss';

import { useEffect, useRef, useState } from 'react';

import { ConnectionManager } from 'TouchFree/Connection/ConnectionManager';
import {
    ActionCode,
    CommunicationWrapper,
    HandRenderDataStateRequest,
} from 'TouchFree/Connection/TouchFreeServiceTypes';
import { HandDataManager } from 'TouchFree/Plugins/HandDataManager';

import SwapMainLensIcon from 'Images/Camera/Swap_Main_Lens_Icon.svg';

import { HandSvg } from 'Components/Controls/HandsSvg';
import { HandSvgCoordinate } from 'Components/Controls/HandsSvg';

import MaskingOption from './MaskingOptions';
import MaskingSlider, { SliderDirection } from './MaskingSlider';
import { displayLensFeeds } from './displayLensFeeds';

interface HandRenderState {
    handOne: any;
    handTwo: any;
}

enum Lens {
    Left,
    Right,
}

const MaskingScreen = () => {
    const [handData, setHandData] = useState<HandRenderState>({
        handOne: {
            indexTip: new HandSvgCoordinate(40, 10, 1),
            indexKnuckle: new HandSvgCoordinate(40, 90, 1),
            middleTip: new HandSvgCoordinate(70, 10, 1),
            middleKnuckle: new HandSvgCoordinate(70, 90, 1),
            ringTip: new HandSvgCoordinate(100, 10, 1),
            ringKnuckle: new HandSvgCoordinate(100, 90, 1),
            littleTip: new HandSvgCoordinate(130, 10, 1),
            littleKnuckle: new HandSvgCoordinate(130, 90, 1),
            thumbTip: new HandSvgCoordinate(10, 40, 1),
            thumbKnuckle: new HandSvgCoordinate(60, 160, 1),
            wrist: new HandSvgCoordinate(80, 160, 1),
            dotColor: 'blue',
        },
        handTwo: {
            indexTip: new HandSvgCoordinate(40, 10, 1),
            indexKnuckle: new HandSvgCoordinate(40, 90, 1),
            middleTip: new HandSvgCoordinate(70, 10, 1),
            middleKnuckle: new HandSvgCoordinate(70, 90, 1),
            ringTip: new HandSvgCoordinate(100, 10, 1),
            ringKnuckle: new HandSvgCoordinate(100, 90, 1),
            littleTip: new HandSvgCoordinate(130, 10, 1),
            littleKnuckle: new HandSvgCoordinate(130, 90, 1),
            thumbTip: new HandSvgCoordinate(10, 40, 1),
            thumbKnuckle: new HandSvgCoordinate(60, 160, 1),
            wrist: new HandSvgCoordinate(80, 160, 1),
            dotColor: 'red',
        },
    });
    // ===== State =====
    const [mainLens, setMainLens] = useState<Lens>(Lens.Left);
    const [isSubFeedHovered, setIsSubFeedHovered] = useState<boolean>(false);
    // Config options
    const [isCamReversed, _setIsCamReversed] = useState<boolean>(false);
    const [showOverexposed, _setShowOverexposed] = useState<boolean>(false);
    const [isFrameProcessing, _setIsFrameProcessing] = useState<boolean>(false);

    // ===== State Refs =====
    // Refs to be able to use current state in eventListeners
    const isCamReversedRef = useRef(isCamReversed);
    const showOverexposedRef = useRef(showOverexposed);
    const successfullySubscribed = useRef<boolean>(false);
    const isFrameProcessingRef = useRef<boolean>(isFrameProcessing);

    // ===== State Setters =====
    const setIsCameraReversed = (value: boolean) => {
        _setIsCamReversed(value);
        isCamReversedRef.current = value;
    };
    const setShowOverexposedAreas = (value: boolean) => {
        _setShowOverexposed(value);
        showOverexposedRef.current = value;
    };
    const setIsFrameProcessing = (value: boolean) => {
        _setIsFrameProcessing(value);
        isFrameProcessingRef.current = value;
    };

    // ===== Canvas Refs =====
    const leftLensRef = useRef<HTMLCanvasElement>(null);
    const rightLensRef = useRef<HTMLCanvasElement>(null);

    // ===== Variables =====
    const byteConversionArray = new Uint32Array(256);
    const byteConversionArrayOverExposed = new Uint32Array(256);
    const sliderDirections: SliderDirection[] = ['left', 'right', 'top', 'bottom'];

    // ===== UseEffect =====
    useEffect(() => {
        for (let i = 0; i < 256; i++) {
            byteConversionArray[i] = (255 << 24) | (i << 16) | (i << 8) | i;
            // -13434625 = #FFFF0033 in signed 2's complement
            byteConversionArrayOverExposed[i] = i > 128 ? -13434625 : byteConversionArray[i];
        }

        const socket = new WebSocket('ws://127.0.0.1:1024');
        socket.binaryType = 'arraybuffer';

        socket.addEventListener('open', openHandler);
        socket.addEventListener('message', (event) => messageHandler(socket, event));

        return () => {
            socket.removeEventListener('open', openHandler);
            socket.removeEventListener('message', (event) => messageHandler(socket, event));
        };
    }, []);

    // ===== EventListeners =====
    const openHandler = () => {
        console.log('WebSocket open');
    };

    const messageHandler = (socket: WebSocket, event: MessageEvent) => {
        if (
            isFrameProcessingRef.current ||
            !leftLensRef.current ||
            !rightLensRef.current ||
            typeof event.data == 'string'
        ) {
            return;
        }

        setIsFrameProcessing(true);

        if (!successfullySubscribed.current) {
            socket.send(JSON.stringify({ type: 'SubscribeImageStreaming' }));
        }

        const data = new DataView(event.data);
        if (data.getUint8(0) === 1) {
            successfullySubscribed.current = true;

            displayLensFeeds(
                data,
                leftLensRef.current,
                rightLensRef.current,
                isCamReversedRef.current,
                showOverexposedRef.current ? byteConversionArrayOverExposed : byteConversionArray
            );
        }

        // Settimeout with 32ms for ~30fps if we have the performance
        setTimeout(() => {
            setIsFrameProcessing(false);
        }, 32);
    };

    const handleTFInput = (evt: CustomEvent<any>): void => {
        if (evt.detail?.Hands) {
            const handOne = evt.detail.Hands[0];
            const handTwo = evt.detail.Hands[1];
            const updatedHandData = {
                handOne: {},
                handTwo: {},
            };

            if (handOne) {
                updatedHandData.handOne = handToSvgData(handOne, 0);
            }
            if (handTwo) {
                updatedHandData.handTwo = handToSvgData(handTwo, 1);
            }
            setHandData(updatedHandData);
        }
    };

    useEffect(() => {
        HandDataManager.instance.addEventListener('TransmitHandData', handleTFInput as EventListener);

        return () => {
            HandDataManager.instance.removeEventListener('TransmitHandData', handleTFInput as EventListener);
        };
    }, []);

    const translateToCoordinate = (coordinate: any) => {
        return new HandSvgCoordinate(800 * (1.13 - (coordinate.X * 1.2)),
            800 * coordinate.Y, coordinate.Z
        );
    };

    const tipJointIndex = 3;
    const knuckleJointIndex = 1;

    const handToSvgData = (hand: any, handIndex: number): any => {
        const indexFinger = hand.Fingers.find((f: any) => f.Type == 1);
        const middleFinger = hand.Fingers.find((f: any) => f.Type == 2);
        const ringFinger = hand.Fingers.find((f: any) => f.Type == 3);
        const littleFinger = hand.Fingers.find((f: any) => f.Type == 4);
        const thumbFinger = hand.Fingers.find((f: any) => f.Type == 0);
        const wrist = hand.WristPosition;
        return {
            indexTip: translateToCoordinate(indexFinger.Bones[tipJointIndex].NextJoint),
            indexKnuckle: translateToCoordinate(indexFinger.Bones[knuckleJointIndex].PrevJoint),
            middleTip: translateToCoordinate(middleFinger.Bones[tipJointIndex].NextJoint),
            middleKnuckle: translateToCoordinate(middleFinger.Bones[knuckleJointIndex].PrevJoint),
            ringTip: translateToCoordinate(ringFinger.Bones[tipJointIndex].NextJoint),
            ringKnuckle: translateToCoordinate(ringFinger.Bones[knuckleJointIndex].PrevJoint),
            littleTip: translateToCoordinate(littleFinger.Bones[tipJointIndex].NextJoint),
            littleKnuckle: translateToCoordinate(littleFinger.Bones[knuckleJointIndex].PrevJoint),
            thumbTip: translateToCoordinate(thumbFinger.Bones[tipJointIndex].NextJoint),
            thumbKnuckle: translateToCoordinate(thumbFinger.Bones[knuckleJointIndex].PrevJoint),
            wrist: translateToCoordinate(wrist),
            primaryHand: hand.primaryHand,
            dotColor: handIndex ? 'blue' : 'red',
        };
    };

    setHandRenderState(true);

    return (
        <div>
            <div className="title-line" style={{ flexDirection: 'column' }}>
                <h1> Camera Masking </h1>
                <p style={{ opacity: '50%' }}>
                    The camera will ignore the areas defined by the boxes that you draw on the camera feed
                </p>
            </div>
            <div className="cam-feed-box--main">
                {sliderDirections.map((direction) => (
                    <MaskingSlider key={direction} direction={direction} />
                ))}
                <canvas ref={mainLens === Lens.Left ? leftLensRef : rightLensRef} />
                <HandSvg key="hand-data-1" data={handData.handOne} />
                <HandSvg key="hand-data-2" data={handData.handTwo} />
                <p>{Lens[mainLens]} Lens</p>
            </div>
            <div className="cam-feeds-bottom-container">
                <div
                    className="cam-feed-box--sub"
                    onPointerEnter={() => setIsSubFeedHovered(true)}
                    onPointerLeave={() => setIsSubFeedHovered(false)}
                    onPointerDown={() => setMainLens(1 - mainLens)}
                >
                    <canvas ref={mainLens === Lens.Left ? rightLensRef : leftLensRef} />
                    <p>{Lens[1 - mainLens]} Lens</p>
                    <span className="sub-feed-overlay" style={{ opacity: isSubFeedHovered ? 0.85 : 0 }}>
                        <div className="sub-feed-overlay--content">
                            <img src={SwapMainLensIcon} alt="Swap Camera Lens icon" />
                            <p>Swap as main lens view</p>
                        </div>
                    </span>
                </div>
                <div className="cam-feeds-options-container">
                    <MaskingOption
                        title="Reverse Camera Orientation"
                        description="Reverse the camera orientation (hand should enter from the bottom)"
                        value={isCamReversed}
                        onChange={setIsCameraReversed}
                    />
                    <MaskingOption
                        title="Display Overexposed Areas"
                        description="Areas, where hand tracking may be an issue will be highlighted"
                        value={showOverexposed}
                        onChange={setShowOverexposedAreas}
                    />
                </div>
            </div>
        </div>
    );
};

const setHandRenderState = (handRenderState: boolean): void => {
    const requestID = uuidgen();

    const content = new HandRenderDataStateRequest(requestID, handRenderState);
    const request = new CommunicationWrapper(ActionCode.SET_HAND_DATA_STREAM_STATE, content);

    const jsonContent = JSON.stringify(request);

    ConnectionManager.serviceConnection()?.SendMessage(jsonContent, requestID, () => {
        return true;
    });
};

export default MaskingScreen;
