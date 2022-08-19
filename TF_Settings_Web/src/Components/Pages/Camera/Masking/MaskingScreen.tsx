import 'Styles/Camera/CameraMasking.scss';

import { useEffect, useMemo, useRef, useState } from 'react';
import { useNavigate } from 'react-router-dom';

import { useStatefulRef } from 'customHooks';

import { TrackingStateResponse } from 'TouchFree/Connection/TouchFreeServiceTypes';
import { TrackingManager } from 'TouchFree/Tracking/TrackingManager';
import { Mask } from 'TouchFree/Tracking/TrackingTypes';

import MaskingLensToggle from './MaskingLensToggle';
import MaskingOption, { MaskingOptionProps } from './MaskingOptions';
import { MaskingSliderDraggable, SliderDirection } from './MaskingSlider';
import { createCanvasUpdate as updateCanvas } from './displayLensFeeds';

export type Lens = 'Left' | 'Right';

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
    const canvasRef = useRef<HTMLCanvasElement>(null);
    const canvasContextRef = useRef<CanvasRenderingContext2D | null>(null);
    const successfullySubscribed = useRef<boolean>(false);
    const frameTimeoutRef = useRef<number>();

    // ===== Hooks =====
    const navigate = useNavigate();

    useEffect(() => {
        if (canvasRef.current) {
            canvasContextRef.current = canvasRef.current.getContext('2d');
        }
        TrackingManager.RequestTrackingState(handleInitialTrackingState);

        const socket = new WebSocket('ws://127.0.0.1:1024');
        socket.binaryType = 'arraybuffer';

        socket.addEventListener('open', handleWSOpen);
        socket.addEventListener('message', (event) => handleMessage(socket, event));
        socket.addEventListener('close', handleWSClose);

        addEventListener('frameRendered', timeoutFrame as EventListener);

        return () => {
            socket.removeEventListener('open', handleWSOpen);
            socket.removeEventListener('message', (event) => handleMessage(socket, event));
            socket.removeEventListener('close', handleWSClose);

            socket.close();

            window.clearTimeout(frameTimeoutRef.current);
            removeEventListener('frameRendered', timeoutFrame as EventListener);
        };
    }, []);

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
        if (!canvasContextRef.current) return;
        if (isFrameProcessing.current || !allowImages.current) return;

        const data = event.data as ArrayBuffer;
        if (!data) return;

        const dataIdentifier = new Uint8Array(data, 0, 1)[0];
        if (dataIdentifier === 1) {
            isFrameProcessing.current = true;
            successfullySubscribed.current = true;

            updateCanvas(
                data,
                canvasContextRef.current,
                mainLens.current,
                isCamReversed.current,
                showOverexposed.current
            );

            dispatchEvent(new CustomEvent('frameRendered'));
        } else if (!successfullySubscribed.current) {
            socket.send(JSON.stringify({ type: 'SubscribeImageStreaming' }));
        }
    };

    const timeoutFrame = () => {
        frameTimeoutRef.current = window.setTimeout(() => {
            isFrameProcessing.current = false;
        }, 32);
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
                <canvas ref={canvasRef} width={'192px'} height={'192px'} />
                <div className="lens-toggle-container">{lensToggles}</div>
            </div>
            <div className="cam-feeds-bottom-container">
                <div className="cam-feeds-options-container">
                    {useMemo(
                        () =>
                            createMaskingOption({
                                title: 'Display Overexposed Areas',
                                description: 'Areas, where hand tracking may be an issue will be highlighted',
                                value: showOverexposed.current,
                                onChange: (value) => (showOverexposed.current = value),
                            }),
                        [showOverexposed.current]
                    )}
                    {useMemo(
                        () =>
                            createMaskingOption({
                                title: 'Allow Images',
                                description: 'Allow images to be sent from the TouchFree Camera',
                                value: allowImages.current,
                                onChange: setAllowImages,
                            }),
                        [allowImages.current]
                    )}
                    {useMemo(
                        () =>
                            createMaskingOption({
                                title: 'Reverse Camera Orientation',
                                description: 'Reverse the camera orientation (hand should enter from the bottom)',
                                value: isCamReversed.current,
                                onChange: setIsCameraReversed,
                                isMouseOnly: true,
                            }),
                        [isCamReversed.current]
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
