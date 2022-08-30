import 'Styles/Camera/CameraMasking.scss';

import { useEffect, useMemo, useRef, useState } from 'react';
import { useNavigate } from 'react-router-dom';

import { useStatefulRef } from 'customHooks';

import { TrackingStateResponse } from 'TouchFree/Connection/TouchFreeServiceTypes';
import { HandDataManager } from 'TouchFree/Plugins/HandDataManager';
import { HandFrame } from 'TouchFree/TouchFreeToolingTypes';
import { TrackingManager } from 'TouchFree/Tracking/TrackingManager';
import { Mask } from 'TouchFree/Tracking/TrackingTypes';

import MaskingLensToggle from './MaskingLensToggle';
import MaskingOption from './MaskingOptions';
import { MaskingSliderDraggable, SliderDirection } from './MaskingSlider';
import { updateCameraCanvas } from './createCameraData';
import { rawHandToHandData, setHandRenderState } from './createHandData';
import { setupRenderScene, updateHandRenders } from './sceneRendering';

export type Lens = 'Left' | 'Right';

const FRAME_PROCESSING_TIMEOUT = 30;

const MaskingScreen = () => {
    // ===== State =====
    const mainLens = useStatefulRef<Lens>('Left');
    // Config options
    const masking = useStatefulRef<Mask>({ left: 0, right: 0, upper: 0, lower: 0 });
    const isCamReversed = useStatefulRef<boolean>(false);
    const allowImages = useStatefulRef<boolean>(false);
    const [allowAnalytics, _setAllowAnalytics] = useState<boolean>(false);

    const showOverexposed = useStatefulRef<boolean>(false);

    const isFrameProcessing = useStatefulRef<boolean>(false);
    const isHandProcessing = useStatefulRef<boolean>(false);

    // ===== State Setters =====
    const setMasking = (direction: SliderDirection, maskingValue: number) => {
        masking.current = { ...masking.current, [direction]: maskingValue };
    };
    const sendMaskingRequest = () => {
        TrackingManager.RequestTrackingChange({ mask: masking.current }, null);
    };
    const clearMasking = () => {
        TrackingManager.RequestTrackingChange({ mask: { left: 0, right: 0, upper: 0, lower: 0 } }, null);
    };

    const setAllowImages = (value: boolean) => {
        allowImages.current = value;
        TrackingManager.RequestTrackingChange({ allowImages: value }, null);
    };
    const setIsCameraReversed = (value: boolean) => {
        isCamReversed.current = value;
        TrackingManager.RequestTrackingChange({ cameraReversed: value }, null);
    };
    const setAllowAnalytics = (value: boolean) => {
        _setAllowAnalytics(value);
        TrackingManager.RequestTrackingChange({ analyticsEnabled: value }, null);
    };

    // ===== Refs =====
    const camFeedRef = useRef<HTMLDivElement>(null);
    const successfullySubscribed = useRef<boolean>(false);
    const frameTimeoutRef = useRef<number>();
    const hasHandRenders = useRef<boolean>(false);
    const handTimeoutRef = useRef<number>();

    // ===== Hooks =====
    const navigate = useNavigate();

    useEffect(() => {
        if (camFeedRef.current) {
            setupRenderScene(camFeedRef.current);
        }
        TrackingManager.RequestTrackingState(handleInitialTrackingState);

        const socket = new WebSocket('ws://127.0.0.1:1024');
        socket.binaryType = 'arraybuffer';

        socket.addEventListener('open', handleWSOpen);
        socket.addEventListener('message', (event) => handleMessage(socket, event));
        socket.addEventListener('close', handleWSClose);

        HandDataManager.instance.addEventListener('TransmitHandData', handleTFInput as EventListener);
        setHandRenderState(true, mainLens.current === 'Left' ? 'left' : 'right');

        return () => {
            socket.removeEventListener('open', handleWSOpen);
            socket.removeEventListener('message', (event) => handleMessage(socket, event));
            socket.removeEventListener('close', handleWSClose);

            socket.close();

            HandDataManager.instance.removeEventListener('TransmitHandData', handleTFInput as EventListener);
            setHandRenderState(false, mainLens.current === 'Left' ? 'left' : 'right');
            window.clearTimeout(frameTimeoutRef.current);
            window.clearTimeout(handTimeoutRef.current);
        };
    }, []);

    useEffect(() => setHandRenderState(true, mainLens.current === 'Left' ? 'left' : 'right'), [mainLens.current]);

    // ===== Event Handlers =====
    const handleInitialTrackingState = (state: TrackingStateResponse) => {
        const configAllowImages = state.allowImages?.content;
        if (configAllowImages) {
            allowImages.current = configAllowImages;
        }

        const configIsCamReversed = state.cameraReversed?.content;
        if (configIsCamReversed) {
            isCamReversed.current = configIsCamReversed;
        }

        const configAnalytics = state.analyticsEnabled?.content;
        if (configAnalytics) {
            _setAllowAnalytics(configAnalytics);
        }

        const configMasking = state.mask?.content;
        if (configMasking) {
            masking.current = {
                left: configMasking.left,
                right: configMasking.right,
                upper: configMasking.upper,
                lower: configMasking.lower,
            };
        }
    };

    const handleWSOpen = () => {
        console.log('Connected to Tracking Service');
    };
    const handleWSClose = () => {
        console.log('Disconnected from Tracking Service');
        navigate('../');
    };

    const handleMessage = (socket: WebSocket, event: MessageEvent) => {
        if (isFrameProcessing.current || !allowImages.current) return;

        const data = event.data as ArrayBuffer;
        if (!data) return;

        const dataIdentifier = new Uint8Array(data, 0, 1)[0];
        if (dataIdentifier === 1) {
            isFrameProcessing.current = true;
            successfullySubscribed.current = true;

            frameTimeoutRef.current = window.setTimeout(() => {
                updateCameraCanvas(data, mainLens.current, isCamReversed.current, showOverexposed.current);
                isFrameProcessing.current = false;
            }, FRAME_PROCESSING_TIMEOUT * 2);
        } else if (!successfullySubscribed.current) {
            socket.send(JSON.stringify({ type: 'SubscribeImageStreaming' }));
        }
    };

    const handleTFInput = (evt: CustomEvent<HandFrame>): void => {
        if (isHandProcessing.current || !successfullySubscribed.current) return;

        isHandProcessing.current = true;

        const hands = evt.detail?.Hands;
        if (hasHandRenders.current || hands.length > 0) {
            const handOne = hands[0];
            const handTwo = hands[1];
            const convertedHandOne = handOne ? rawHandToHandData(handOne) : undefined;
            const convertedHandTwo = handTwo ? rawHandToHandData(handTwo) : undefined;

            hasHandRenders.current = convertedHandOne !== undefined || convertedHandTwo !== undefined;

            updateHandRenders({ one: convertedHandOne, two: convertedHandTwo });
        }

        // Ignore any messages for a short period to allow clearing of message handling
        handTimeoutRef.current = window.setTimeout(() => {
            isHandProcessing.current = false;
        }, FRAME_PROCESSING_TIMEOUT);
    };

    // ===== Components =====
    const sliders = useMemo(
        () =>
            Object.entries(masking.current).map((sliderInfo) => (
                <MaskingSliderDraggable
                    key={sliderInfo[0]}
                    direction={sliderInfo[0] as SliderDirection}
                    maskingValue={sliderInfo[1]}
                    canvasInfo={{ size: 800, topOffset: 50, leftOffset: 100 }}
                    clearMasking={clearMasking}
                    onDrag={setMasking}
                    onDragEnd={sendMaskingRequest}
                />
            )),
        [masking.current]
    );

    const lensToggles = useMemo(
        () => (
            <>
                <MaskingLensToggle
                    lens={'Left'}
                    isMainLens={mainLens.current === 'Left'}
                    setMainLens={(value: Lens) => (mainLens.current = value)}
                />
                <MaskingLensToggle
                    lens={'Right'}
                    isMainLens={mainLens.current === 'Right'}
                    setMainLens={(value: Lens) => (mainLens.current = value)}
                />
            </>
        ),
        [mainLens.current]
    );

    return (
        <div>
            <div className="title-line" style={{ flexDirection: 'column' }}>
                <h1> Camera Masking </h1>
                <p style={{ opacity: '50%' }}>
                    The camera will ignore the areas defined by the boxes that you draw on the camera feed
                </p>
            </div>
            <div className="cam-feed-box--main">
                {sliders}
                <div className="cam-feed-box-feed">
                    <div className="cam-feed-box-feed--render" ref={camFeedRef} />
                </div>
                <div className="lens-toggle-container">{lensToggles}</div>
            </div>
            <div className="cam-feeds-bottom-container">
                <div className="cam-feeds-options-container">
                    {useMemo(
                        () => (
                            <MaskingOption
                                title="Display Overexposed Areas"
                                description="Areas, where hand tracking may be an issue will be highlighted"
                                value={showOverexposed.current}
                                onChange={(value) => (showOverexposed.current = value)}
                            />
                        ),
                        [showOverexposed.current]
                    )}
                    {useMemo(
                        () => (
                            <MaskingOption
                                title="Allow Images"
                                description="Allow images to be sent from the TouchFree Camera"
                                value={allowImages.current}
                                onChange={setAllowImages}
                            />
                        ),
                        [allowImages.current]
                    )}
                    {useMemo(
                        () => (
                            <MaskingOption
                                title="Reverse Camera Orientation"
                                description="Reverse the camera orientation (hand should enter from the bottom)"
                                value={isCamReversed.current}
                                onChange={setIsCameraReversed}
                                isMouseOnly
                            />
                        ),
                        [isCamReversed.current]
                    )}
                    {useMemo(
                        () => (
                            <MaskingOption
                                title="Allow Analytics"
                                description="Allow analytic data to be collected"
                                value={allowAnalytics}
                                onChange={setAllowAnalytics}
                            />
                        ),
                        [allowAnalytics]
                    )}
                </div>
            </div>
        </div>
    );
};

export default MaskingScreen;
