import 'Styles/Camera/CameraMasking.scss';

import { useEffect, useRef, useState } from 'react';

import { TrackingStateResponse } from 'TouchFree/Connection/TouchFreeServiceTypes';
import { TrackingManager } from 'TouchFree/Tracking/TrackingManager';
import { Mask } from 'TouchFree/Tracking/TrackingTypes';

import SwapMainLensIcon from 'Images/Camera/Swap_Main_Lens_Icon.svg';

import MaskingOption from './MaskingOptions';
import { MaskingSlider, MaskingSliderDraggable, SliderDirection } from './MaskingSlider';
import { displayLensFeeds } from './displayLensFeeds';

enum Lens {
    Left,
    Right,
}

const MaskingScreen = () => {
    // ===== State =====
    const [mainLens, setMainLens] = useState<Lens>(Lens.Left);
    const [isSubFeedHovered, setIsSubFeedHovered] = useState<boolean>(false);
    // Config options
    const [masking, _setMasking] = useState<Mask>({ left: 0, right: 0, upper: 0, lower: 0 });
    const [isCamReversed, _setIsCamReversed] = useState<boolean>(false);
    const [allowImages, _setAllowImages] = useState<boolean>(false);
    const [allowAnalytics, _setAllowAnalytics] = useState<boolean>(false);

    const [showOverexposed, _setShowOverexposed] = useState<boolean>(false);

    const [isFrameProcessing, _setIsFrameProcessing] = useState<boolean>(false);

    // ===== State Refs =====
    // Refs to be able to use current state in eventListeners
    const maskingRef = useRef<Mask>(masking);
    const isCamReversedRef = useRef<boolean>(isCamReversed);
    const allowImagesRef = useRef<boolean>(allowImages);
    const showOverexposedRef = useRef<boolean>(showOverexposed);
    const successfullySubscribed = useRef<boolean>(false);
    const isFrameProcessingRef = useRef<boolean>(isFrameProcessing);
    const timeoutRef = useRef<number>();

    // ===== State Setters =====
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
    const leftLensRef = useRef<HTMLCanvasElement>(null);
    const rightLensRef = useRef<HTMLCanvasElement>(null);

    // ===== Variables =====
    const byteConversionArray = new Uint32Array(256);
    const byteConversionArrayOverExposed = new Uint32Array(256);

    // ===== UseEffect =====
    useEffect(() => {
        TrackingManager.RequestTrackingState(handleInitialTrackingState);

        for (let i = 0; i < 256; i++) {
            byteConversionArray[i] = (255 << 24) | (i << 16) | (i << 8) | i;
            // -13434625 = #FFFF0033 in signed 2's complement
            byteConversionArrayOverExposed[i] = i > 128 ? -13434625 : byteConversionArray[i];
        }

        const socket = new WebSocket('ws://127.0.0.1:1024');
        socket.binaryType = 'arraybuffer';

        socket.addEventListener('open', handleWSOpen);
        socket.addEventListener('message', (event) => handleMessage(socket, event));

        return () => {
            socket.removeEventListener('open', handleWSOpen);
            socket.removeEventListener('message', (event) => handleMessage(socket, event));
            window.clearTimeout(timeoutRef.current);
        };
    }, []);

    // ===== Event Handlers =====
    const handleInitialTrackingState = (state: TrackingStateResponse) => {
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
        console.log('WebSocket open');
    };

    const handleMessage = (socket: WebSocket, event: MessageEvent) => {
        if (!leftLensRef.current || !rightLensRef.current) return;
        if (isFrameProcessingRef.current || !allowImagesRef.current) return;

        if (typeof event.data == 'string') return;

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
        timeoutRef.current = window.setTimeout(() => {
            setIsFrameProcessing(false);
        }, 32);
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
                        canvasInfo={{ size: 800, offset: 100 }}
                        clearMasking={clearMasking}
                        onDrag={setMasking}
                        onDragEnd={sendMaskingRequest}
                    />
                ))}
                <canvas ref={mainLens === Lens.Left ? leftLensRef : rightLensRef} />
                <p>{Lens[mainLens]} Lens</p>
            </div>
            <div className="cam-feeds-bottom-container">
                <div
                    className="cam-feed-box--sub"
                    onPointerEnter={() => setIsSubFeedHovered(true)}
                    onPointerLeave={() => setIsSubFeedHovered(false)}
                    onPointerDown={() => setMainLens(1 - mainLens)}
                >
                    {Object.entries(masking).map((sliderInfo) => (
                        <MaskingSlider
                            key={sliderInfo[0]}
                            direction={sliderInfo[0] as SliderDirection}
                            maskingValue={sliderInfo[1]}
                            canvasInfo={{ size: 360, offset: 60 }}
                        />
                    ))}
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
