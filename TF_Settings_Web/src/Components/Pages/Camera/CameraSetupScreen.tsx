import 'Styles/Camera/Camera.css';

import React, { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';

import { HandDataManager } from 'TouchFree/Plugins/HandDataManager';

import ManualSetupIcon from 'Images/Camera/Manual_Setup_Icon.svg';
import QuickSetupIcon from 'Images/Camera/Quick_Setup_Icon.svg';

import { HandSvg, HandSvgCoordinate } from 'Components/Controls/HandSvg';
import IconTextButton from 'Components/Controls/IconTextButton';

interface HandRenderState {
    handOne: any;
    handTwo: any;
}

type CameraType = 'left' | 'right';

const CameraSetupScreen = () => {
    const topVideoRef = React.useRef<HTMLCanvasElement>(null);
    const botVideoRef = React.useRef<HTMLCanvasElement>(null);

    const frameCount = React.useRef<number>(0);

    useEffect(() => {
        const socket = new WebSocket('ws://127.0.0.1:1024');
        socket.binaryType = 'arraybuffer';

        socket.addEventListener('open', () => {
            console.log('open');
            socket.send(JSON.stringify({ type: 'SubscribeImageStreaming' }));
        });

        // setInterval(() => {
        //     console.log(frameCount.current);
        //     frameCount.current = 0;
        // }, 1000);

        socket.addEventListener('message', (event) => {
            if (!topVideoRef.current || !botVideoRef.current || typeof event.data == 'string') return;
            frameCount.current++;
            if (frameCount.current > 1) {
                if (frameCount.current == 10) {
                    frameCount.current = 0;
                }
                return;
            }
            const data = new DataView(event.data);
            if (data.getUint8(0) === 1) {
                displayCameraFeed(data, 'left', topVideoRef.current);
                displayCameraFeed(data, 'right', botVideoRef.current);
            }
        });
    }, []);

    const navigate = useNavigate();
    const [handData, setHandData] = React.useState<HandRenderState>({
        handOne: {
            indexTip: new HandSvgCoordinate(40, 10),
            indexKnuckle: new HandSvgCoordinate(40, 90),
            middleTip: new HandSvgCoordinate(70, 10),
            middleKnuckle: new HandSvgCoordinate(70, 90),
            ringTip: new HandSvgCoordinate(100, 10),
            ringKnuckle: new HandSvgCoordinate(100, 90),
            littleTip: new HandSvgCoordinate(130, 10),
            littleKnuckle: new HandSvgCoordinate(130, 90),
            thumbTip: new HandSvgCoordinate(10, 40),
            wrist: new HandSvgCoordinate(80, 160),
            dotColor: 'blue',
        },
        handTwo: {
            indexTip: new HandSvgCoordinate(40, 10),
            indexKnuckle: new HandSvgCoordinate(40, 90),
            middleTip: new HandSvgCoordinate(70, 10),
            middleKnuckle: new HandSvgCoordinate(70, 90),
            ringTip: new HandSvgCoordinate(100, 10),
            ringKnuckle: new HandSvgCoordinate(100, 90),
            littleTip: new HandSvgCoordinate(130, 10),
            littleKnuckle: new HandSvgCoordinate(130, 90),
            thumbTip: new HandSvgCoordinate(10, 40),
            wrist: new HandSvgCoordinate(80, 160),
            dotColor: 'red',
        },
    });

    // useEffect(() => {

    //     // return () => {
    //     //     HandDataManager.instance.removeEventListener('TransmitHandData', handleTFInput as EventListener);
    //     // };
    // }, []);

    const handleTFInput = (evt: CustomEvent<any>): void => {
        if (evt.detail?.Hands) {
            const handOne = evt.detail.Hands[0];
            const handTwo = evt.detail.Hands[1];
            const updatedHandData = {
                handOne: {},
                handTwo: {},
            };

            if (handOne) {
                updatedHandData.handOne = handToSvgData(handOne);
            }
            if (handTwo) {
                updatedHandData.handTwo = handToSvgData(handTwo);
            }
            console.log(evt.detail?.Hands);
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
        return new HandSvgCoordinate(330-coordinate.X, coordinate.Y - 30);
    };

    const tipJointIndex = 3;
    const knuckleJointIndex = 1;

    const handToSvgData = (hand: any): any => {
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
            wrist: translateToCoordinate(wrist),
            dotColor: 'blue',
        };
    };

    return (
        <div>
            <div className="titleLine">
                <h1> Camera Setup </h1>
            </div>
            <div className="IconTextButtonDiv">
                <IconTextButton
                    buttonStyle={{ width: '63.75%' }}
                    icon={QuickSetupIcon}
                    alt="Icon for Quick Setup option"
                    iconStyle={{ margin: '30px 0px', height: '250px' }}
                    title="Auto Calibration"
                    text="Our automatic calibration enables you to set up quickly"
                    onClick={() => navigate('quick')}
                />
                <IconTextButton
                    buttonStyle={{ width: '33.75%' }}
                    icon={ManualSetupIcon}
                    alt="Icon for Manual Setup option"
                    iconStyle={{ margin: '65px 0px', height: '180px' }}
                    title="Manual Calibration"
                    text="Full control of your calibration"
                    onClick={() => navigate('manual')}
                />
            </div>
            <canvas ref={topVideoRef} style={{ width: '400px', height: '400px', margin: '50px' }} />
            <HandSvg key="hand-data-1" data={handData.handOne} />
            <canvas ref={botVideoRef} style={{ width: '400px', height: '400px', margin: '50px' }} />
            <HandSvg key="hand-data-2" data={handData.handTwo} />
        </div>
    );
};

export default CameraSetupScreen;

// Decimal in signed 2's complement
// const OVEREXPOSED_THRESHOLD = -12566464; //#FF404040;
// const OVEREXPOSED_THRESHOLD = -8355712; //#FF808080;
const OVEREXPOSED_THRESHOLD = -6250336; //#FFA0A0A0;
// const OVEREXPOSED_THRESHOLD = -4144960; //#FFC0C0C0;

const displayCameraFeed = (data: DataView, camera: CameraType, canvas: HTMLCanvasElement) => {
    const context = canvas.getContext('2d');
    if (!context) return;

    const width = data.getUint32(1);
    const cameraHeight = data.getUint32(5) / 2;

    const buf = new ArrayBuffer(width * cameraHeight * 4);
    const buf8 = new Uint8ClampedArray(buf);
    const buf32 = new Uint32Array(buf);

    const offset = camera === 'right' ? 0 : width * cameraHeight;

    // Set first row to black pixels to remove the flashing magic line
    buf32.fill(0xff000000, 0, width);
    for (let i = width; i < width * cameraHeight; i++) {
        const px = data.getUint8(9 + i + offset);
        const hexColor = (255 << 24) | (px << 16) | (px << 8) | px;
        buf32[i] = hexColor < OVEREXPOSED_THRESHOLD ? hexColor : 0xffffff00;
    }

    canvas.width = width;
    canvas.height = cameraHeight;
    context.putImageData(new ImageData(buf8, width, cameraHeight), 0, 0);
};
