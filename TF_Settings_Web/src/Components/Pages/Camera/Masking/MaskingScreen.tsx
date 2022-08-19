import 'Styles/Camera/CameraMasking.scss';

import { useEffect, useMemo, useRef, useState } from 'react';
import { useNavigate } from 'react-router-dom';

import { TrackingStateResponse } from 'TouchFree/Connection/TouchFreeServiceTypes';
import { TrackingManager } from 'TouchFree/Tracking/TrackingManager';
import { Mask } from 'TouchFree/Tracking/TrackingTypes';

import MaskingLensToggle from './MaskingLensToggle';
import MaskingOption, { MaskingOptionProps } from './MaskingOptions';
import { MaskingSliderDraggable, SliderDirection } from './MaskingSlider';
import { createCanvasUpdate as updateCanvas } from './displayLensFeeds';

export type Lens = 'Left' | 'Right';

const useStatefulRef = function <T>(initialValue: T): {
    current: T;
} {
    const [currentVal, setCurrentVal] = useState<T>(initialValue);
    const valRef = useRef<T>(currentVal);

    const [statefulRef] = useState({
        get current() {
            return valRef.current;
        },
        set current(value: T) {
            valRef.current = value;
            setCurrentVal(value);
        },
    });

    return statefulRef;
};

const MaskingScreen = () => {
    // ===== State =====
    const mainLens = useStatefulRef<Lens>('Left');
    // Config options
    const [masking, _setMasking] = useState<Mask>({ left: 0, right: 0, upper: 0, lower: 0 });
    const [isCamReversed, _setIsCamReversed] = useState<boolean>(false);
    const [allowImages, _setAllowImages] = useState<boolean>(false);
    const [allowAnalytics, _setAllowAnalytics] = useState<boolean>(false);

    const [showOverexposed, _setShowOverexposed] = useState<boolean>(false);

    const [isFrameProcessing, _setIsFrameProcessing] = useState<boolean>(false);

    // ===== State Refs =====
    // Refs to be able to use current state in eventListeners
    // const mainLensRef = useRef<Lens>(mainLens);
    const maskingRef = useRef<Mask>(masking);
    const isCamReversedRef = useRef<boolean>(isCamReversed);
    const allowImagesRef = useRef<boolean>(allowImages);
    const showOverexposedRef = useRef<boolean>(showOverexposed);
    const successfullySubscribed = useRef<boolean>(false);
    const trackingIntervalRef = useRef<number>();
    const isFrameProcessingRef = useRef<boolean>(isFrameProcessing);
    const frameTimeoutRef = useRef<number>();

    // ===== State Setters =====
    // const setMainLens = (value: Lens) => {
    //     mainLensRef.current = value;
    //     _setMainLens(value);
    // };
    const setMasking = (direction: SliderDirection, maskingValue: number) => {
        const mask: Mask = { ...masking, [direction]: maskingValue };
        maskingRef.current = mask;
        _setMasking(mask);
    };
    const sendMaskingRequest = () => {
        TrackingManager.RequestTrackingChange({ mask: maskingRef.current }, null);
    };
    const clearMasking = () => {
        TrackingManager.RequestTrackingChange({ mask: { left: 0, right: 0, upper: 0, lower: 0 } }, null);
    };

    const setAllowImages = (value: boolean) => {
        _setAllowImages(value);
        allowImagesRef.current = value;
        TrackingManager.RequestTrackingChange({ allowImages: value }, null);
    };
    const setIsCameraReversed = (value: boolean) => {
        _setIsCamReversed(value);
        isCamReversedRef.current = value;
        TrackingManager.RequestTrackingChange({ cameraReversed: value }, null);
    };
    const setAllowAnalytics = (value: boolean) => {
        _setAllowAnalytics(value);
        TrackingManager.RequestTrackingChange({ analyticsEnabled: value }, null);
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
    const mainCanvasRef = useRef<HTMLCanvasElement>(null);

    // ===== UseEffect =====
    const navigate = useNavigate();

    useEffect(() => {
        TrackingManager.RequestTrackingState(handleInitialTrackingState);

        const socket = new WebSocket('ws://127.0.0.1:1024');
        socket.binaryType = 'arraybuffer';

        socket.addEventListener('open', handleWSOpen);
        socket.addEventListener('message', (event) => handleMessage(socket, event));
        socket.addEventListener('close', handleWSClose);

        addEventListener('updateCanvas', timeoutFrame as EventListener);

        return () => {
            socket.removeEventListener('open', handleWSOpen);
            socket.removeEventListener('message', (event) => handleMessage(socket, event));
            socket.removeEventListener('close', handleWSClose);

            socket.close();

            removeEventListener('updateCanvas', timeoutFrame as EventListener);
            window.clearTimeout(frameTimeoutRef.current);
        };
    }, []);

    // ===== Event Handlers =====
    const handleInitialTrackingState = (state: TrackingStateResponse) => {
        window.clearInterval(trackingIntervalRef.current);
        const allowImages = state.allowImages?.content;
        if (allowImages) {
            _setAllowImages(allowImages);
            allowImagesRef.current = allowImages;
        }

        const isCamReversed = state.cameraReversed?.content;
        if (isCamReversed) {
            _setIsCamReversed(isCamReversed);
            isCamReversedRef.current = isCamReversed;
        }

        const analytics = state.analyticsEnabled?.content;
        if (analytics) {
            _setAllowAnalytics(analytics);
        }

        const masking = state.mask?.content;
        if (masking) {
            _setMasking({ left: masking.left, right: masking.right, upper: masking.upper, lower: masking.lower });
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
        if (!mainCanvasRef.current) return;
        if (isFrameProcessingRef.current || !allowImagesRef.current) return;

        const data = event.data as ArrayBuffer;
        if (!data) return;

        const dataIdentifier = new Uint8Array(data, 0, 1)[0];

        if (dataIdentifier === 1) {
            setIsFrameProcessing(true);
            successfullySubscribed.current = true;

            const updateEvent = updateCanvas(
                data,
                mainCanvasRef.current,
                mainLens.current,
                isCamReversedRef.current,
                showOverexposedRef.current
            );

            if (updateEvent) {
                console.log('DISPATCH');
                console.log(mainLens.current);
                dispatchEvent(updateEvent);
            }
        } else if (!successfullySubscribed.current) {
            socket.send(JSON.stringify({ type: 'SubscribeImageStreaming' }));
        }
    };

    const timeoutFrame = () => {
        frameTimeoutRef.current = window.setTimeout(() => {
            // console.log('timeout');
            setIsFrameProcessing(false);
        }, 100);
    };

    // ===== Components =====
    const sliders = useMemo(
        () =>
            Object.entries(masking).map((sliderInfo) => (
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
        [masking]
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

    const createMaskingOption = (input: MaskingOptionProps) => {
        return (
            <MaskingOption
                title={input.title}
                description={input.description}
                value={input.value}
                onChange={input.onChange}
                isMouseOnly={input.isMouseOnly}
            />
        );
    };

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
                <canvas ref={mainCanvasRef} />
                <div className="lens-toggle-container">{lensToggles}</div>
            </div>
            <div className="cam-feeds-bottom-container">
                <div className="cam-feeds-options-container">
                    {useMemo(
                        () =>
                            createMaskingOption({
                                title: 'Display Overexposed Areas',
                                description: 'Areas, where hand tracking may be an issue will be highlighted',
                                value: showOverexposed,
                                onChange: setShowOverexposedAreas,
                            }),
                        [showOverexposed]
                    )}
                    {useMemo(
                        () =>
                            createMaskingOption({
                                title: 'Allow Images',
                                description: 'Allow images to be sent from the TouchFree Camera',
                                value: allowImages,
                                onChange: setAllowImages,
                            }),
                        [allowImages]
                    )}
                    {useMemo(
                        () =>
                            createMaskingOption({
                                title: 'Reverse Camera Orientation',
                                description: 'Reverse the camera orientation (hand should enter from the bottom)',
                                value: isCamReversed,
                                onChange: setIsCameraReversed,
                                isMouseOnly: true,
                            }),
                        [isCamReversed]
                    )}
                    {useMemo(
                        () =>
                            createMaskingOption({
                                title: 'Allow Analytics',
                                description: 'Allow analytic data to be collected',
                                value: allowAnalytics,
                                onChange: setAllowAnalytics,
                            }),
                        [allowAnalytics]
                    )}
                </div>
            </div>
        </div>
    );
};

export default MaskingScreen;
