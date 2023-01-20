import classnames from 'classnames/bind';
import { WebGLRenderer } from 'three';

import styles from './CameraMasking.module.scss';

import { useEffect, useMemo, useRef, useState } from 'react';

import { useIsLandscape, useStatefulRef } from '@/customHooks';

import { TrackingStateResponse } from 'TouchFree/src/Connection/TouchFreeServiceTypes';
import { HandDataManager } from 'TouchFree/src/Plugins/HandDataManager';
import { TrackingManager } from 'TouchFree/src/Tracking/TrackingManager';
import { Mask } from 'TouchFree/src/Tracking/TrackingTypes';

import { BackButton } from '@/Components';

import MaskingLensToggle from './MaskingLensToggle';
import MaskingOption from './MaskingOptions';
import { MaskingSliderDraggable, SliderDirection } from './MaskingSlider';
import { updateCameraCanvas } from './createCameraData';
import { HandState, rawHandToHandData, setHandRenderState } from './createHandData';
import { setupRenderScene } from './sceneRendering';

const classes = classnames.bind(styles);

export type Lens = 'Left' | 'Right';

const FRAME_PROCESSING_TIMEOUT = 0;

const MaskingScreen: React.FC = () => {
    // ===== State =====
    const mainLens = useStatefulRef<Lens>('Left');
    const handState = useStatefulRef<HandState>({});
    // Config options
    const masking = useStatefulRef<Mask>({ left: 0, right: 0, upper: 0, lower: 0 });
    const isCamReversed = useStatefulRef<boolean>(false);
    const allowImages = useStatefulRef<boolean>(false);
    const [allowAnalytics, _setAllowAnalytics] = useState<boolean>(false);

    const isLandscape = useIsLandscape();

    const showOverexposed = useStatefulRef<boolean>(false);

    const isHandProcessing = useStatefulRef<boolean>(false);

    const renderer = useRef<WebGLRenderer>();

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
    const onResize = () => {
        if (!camFeedRef.current) return;
        const { clientWidth, clientHeight } = camFeedRef.current;
        renderer.current?.setSize(clientWidth, clientHeight);
    };

    // ===== Refs =====
    const camFeedRef = useRef<HTMLDivElement>(null);
    const handTimeoutRef = useRef<number>();

    // ===== Hooks =====

    useEffect(() => {
        if (camFeedRef.current) {
            renderer.current = setupRenderScene(camFeedRef.current);
        }
        TrackingManager.RequestTrackingState(handleInitialTrackingState);

        HandDataManager.instance.addEventListener('TransmitHandData', handleTFInput as EventListener);
        setHandRenderState(true, mainLens.current === 'Left' ? 'left' : 'right');

        window.addEventListener('resize', onResize);

        return () => {
            HandDataManager.instance.removeEventListener('TransmitHandData', handleTFInput as EventListener);
            setHandRenderState(false, mainLens.current === 'Left' ? 'left' : 'right');
            window.clearTimeout(handTimeoutRef.current);
            window.removeEventListener('resize', onResize);
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

    const handleTFInput = (evt: CustomEvent<ArrayBuffer>): void => {
        if (isHandProcessing.current) return;

        isHandProcessing.current = true;

        const buffer = evt.detail;
        if (!buffer || buffer.byteLength < 8) {
            isHandProcessing.current = false;
            return;
        }

        const imageArraySize = new Int32Array(buffer, 4, 1)[0];

        if (buffer.byteLength < 8 + imageArraySize) {
            isHandProcessing.current = false;
            return;
        }

        try {
            parseAndUpdateHandState(buffer, 8 + imageArraySize);

            if (imageArraySize > 0) {
                updateCameraCanvas(
                    buffer.slice(8, imageArraySize),
                    isCamReversed.current,
                    showOverexposed.current,
                    handState.current
                );
            }

            // Ignore any messages for a short period to allow clearing of message handling
            handTimeoutRef.current = window.setTimeout(() => {
                isHandProcessing.current = false;
            }, FRAME_PROCESSING_TIMEOUT);
        } catch {
            isHandProcessing.current = false;
        }
    };

    const parseAndUpdateHandState = (buffer: ArrayBuffer, offset: number): void => {
        const hands = JSON.parse(String.fromCharCode(...new Uint8Array(buffer, offset)))?.Hands;

        if (hands && (hands.length > 0 || handState.current.one || handState.current.two)) {
            const handOne = hands[0];
            const handTwo = hands[1];
            const convertedHandOne = handOne ? rawHandToHandData(handOne) : undefined;
            const convertedHandTwo = handTwo ? rawHandToHandData(handTwo) : undefined;

            handState.current = { one: convertedHandOne, two: convertedHandTwo };
        }
    };

    // ===== Components =====
    const sliders = useMemo(
        () =>
            Object.entries(masking.current).map((sliderInfo) => (
                <MaskingSliderDraggable
                    key={sliderInfo[0]}
                    direction={sliderInfo[0] as SliderDirection}
                    maskingValue={sliderInfo[1]}
                    canvasSize={isLandscape ? innerHeight * 0.55 : innerWidth * 0.96 - innerHeight * 0.1}
                    clearMasking={clearMasking}
                    onDrag={setMasking}
                    onDragEnd={sendMaskingRequest}
                />
            )),
        [masking.current, isLandscape]
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
        <div className={classes('container')}>
            <div className={classes('header')}>
                <div className={classes('title-line')}>
                    <h1> Camera Masking </h1>
                    <p style={{ opacity: '50%' }}>
                        The camera will ignore the areas defined by the boxes that you draw on the camera feed
                    </p>
                </div>
                {isLandscape ? <></> : <BackButton />}
            </div>
            <div className={classes('sub-container')}>
                <div className={classes('cam-feed-box')}>
                    <div className={classes('cam-feed-box-feed')}>
                        <div className={classes('slider-container')}>{sliders}</div>
                        <div className={classes('cam-feed-box-feed--render')} ref={camFeedRef} />
                    </div>
                    <div className={classes('lens-toggle-container')}>{lensToggles}</div>
                </div>
                <div className={classes('cam-feeds-bottom-container')}>
                    <div className={classes('cam-feeds-options-container')}>
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
                        {isLandscape ? <BackButton /> : <></>}
                    </div>
                </div>
            </div>
        </div>
    );
};

export default MaskingScreen;
