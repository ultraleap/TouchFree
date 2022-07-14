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

const CameraSetupScreen = () => {
    const navigate = useNavigate();
    const [handData, setHandData] = React.useState<HandRenderState>({handOne: {
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
        }
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
                handTwo: {}
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
        return new HandSvgCoordinate(200 + (coordinate.X * 400), 200 + (coordinate.Z * 400));
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
                <HandSvg key="hand-data-1" data={handData.handOne} />
                <HandSvg key="hand-data-2" data={handData.handTwo} />
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
        </div>
    );
};

export default CameraSetupScreen;
