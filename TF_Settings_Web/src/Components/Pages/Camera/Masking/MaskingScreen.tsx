import 'Styles/Camera/CameraMasking.scss';

import { useEffect, useRef, useState } from 'react';

import { TrackingStateResponse } from 'TouchFree/Connection/TouchFreeServiceTypes';
import { TrackingManager } from 'TouchFree/Tracking/TrackingManager';
import { Mask } from 'TouchFree/Tracking/TrackingTypes';

import MaskingLensToggle from './MaskingLensToggle';
import MaskingOption from './MaskingOptions';
import { MaskingSliderDraggable, SliderDirection } from './MaskingSlider';
import { displayLensFeeds } from './displayLensFeeds';

export type Lens = 'Left' | 'Right';

const MaskingScreen = () => {
    // ===== State =====
    const [mainLens, _setMainLens] = useState<Lens>('Left');
    // Config options
    const [masking, _setMasking] = useState<Mask>({ left: 0, right: 0, upper: 0, lower: 0 });
    const [isCamReversed, _setIsCamReversed] = useState<boolean>(false);
    const [allowImages, _setAllowImages] = useState<boolean>(false);
    const [allowAnalytics, _setAllowAnalytics] = useState<boolean>(false);

    const [showOverexposed, _setShowOverexposed] = useState<boolean>(false);

    const [isFrameProcessing, _setIsFrameProcessing] = useState<boolean>(false);

    // ===== State Refs =====
    // Refs to be able to use current state in eventListeners
    const mainLensRef = useRef<Lens>(mainLens);
    const maskingRef = useRef<Mask>(masking);
    const isCamReversedRef = useRef<boolean>(isCamReversed);
    const allowImagesRef = useRef<boolean>(allowImages);
    const showOverexposedRef = useRef<boolean>(showOverexposed);
    const successfullySubscribed = useRef<boolean>(false);
    const trackingIntervalRef = useRef<number>();
    const isFrameProcessingRef = useRef<boolean>(isFrameProcessing);
    const frameTimeoutRef = useRef<number>();

    // ===== State Setters =====
    const setMainLens = (value: Lens) => {
        mainLensRef.current = value;
        _setMainLens(value);
    };
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
    useEffect(() => {
        TrackingManager.RequestTrackingState(handleInitialTrackingState);

        const socket = new WebSocket('ws://127.0.0.1:1024');
        socket.binaryType = 'arraybuffer';

        socket.addEventListener('open', handleWSOpen);
        socket.addEventListener('message', (event) => handleMessage(socket, event));

        return () => {
            socket.removeEventListener('open', handleWSOpen);
            socket.removeEventListener('message', (event) => handleMessage(socket, event));
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
        console.log('WebSocket Open');
    };

    const handleMessage = (socket: WebSocket, event: MessageEvent) => {
        if (!mainCanvasRef.current) return;
        if (isFrameProcessingRef.current || !allowImagesRef.current) return;

        if (typeof event.data == 'string') return;

        const dataAsUint8 = new Uint8Array(event.data, 0, 10);

        if (dataAsUint8[0] === 1) {
            setIsFrameProcessing(true);
            successfullySubscribed.current = true;

            displayLensFeeds(
                event.data as ArrayBuffer,
                mainCanvasRef.current,
                mainLensRef.current,
                isCamReversedRef.current,
                showOverexposedRef.current
            );

            // Settimeout with 32ms for ~30fps if we have the performance
            frameTimeoutRef.current = window.setTimeout(() => {
                setIsFrameProcessing(false);
            }, 32);
        } else if (!successfullySubscribed.current) {
            socket.send(JSON.stringify({ type: 'SubscribeImageStreaming' }));
        }
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
                {Object.entries(masking).map((sliderInfo) => (
                    <MaskingSliderDraggable
                        key={sliderInfo[0]}
                        direction={sliderInfo[0] as SliderDirection}
                        maskingValue={sliderInfo[1]}
                        canvasInfo={{ size: 800, topOffset: 50, leftOffset: 100 }}
                        clearMasking={clearMasking}
                        onDrag={setMasking}
                        onDragEnd={sendMaskingRequest}
                    />
                ))}
                <canvas ref={mainCanvasRef} />
                <div className="lens-toggle-container">
                    <MaskingLensToggle lens={'Left'} isMainLens={mainLens === 'Left'} setMainLens={setMainLens} />
                    <MaskingLensToggle lens={'Right'} isMainLens={mainLens === 'Right'} setMainLens={setMainLens} />
                </div>
            </div>
            <div className="cam-feeds-bottom-container">
                <div className="cam-feeds-options-container">
                    <MaskingOption
                        title="Display Overexposed Areas"
                        description="Areas, where hand tracking may be an issue will be highlighted"
                        value={showOverexposed}
                        onChange={setShowOverexposedAreas}
                    />
                    <MaskingOption
                        title="Allow Images"
                        description="Allow images to be sent from the TouchFree Camera"
                        value={allowImages}
                        onChange={setAllowImages}
                    />
                    <MaskingOption
                        title="Reverse Camera Orientation"
                        description="Reverse the camera orientation (hand should enter from the bottom)"
                        value={isCamReversed}
                        onChange={setIsCameraReversed}
                        isMouseOnly
                    />
                    <MaskingOption
                        title="Allow Analytics"
                        description="Allow analytic data to be collected"
                        value={allowAnalytics}
                        onChange={setAllowAnalytics}
                    />
                </div>
            </div>
        </div>
    );
};

export default MaskingScreen;
