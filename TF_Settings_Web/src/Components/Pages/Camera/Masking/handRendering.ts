import { v4 as uuidgen } from 'uuid';

import { Vector } from 'TouchFree/Configuration/ConfigurationTypes';
import { ConnectionManager } from 'TouchFree/Connection/ConnectionManager';
import {
    ActionCode,
    CommunicationWrapper,
    HandRenderDataStateRequest,
} from 'TouchFree/Connection/TouchFreeServiceTypes';
import { RawFinger, RawHand } from 'TouchFree/TouchFreeToolingTypes';

import { HandSvgCoordinate, HandSvgProps } from 'Components/Controls/HandsSvg';

export const defaultHandState = {
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
        primaryHand: true,
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
        primaryHand: false,
        dotColor: 'red',
    },
};

export const setHandRenderState = (handRenderState: boolean, lens: string): void => {
    const requestID = uuidgen();

    const content = new HandRenderDataStateRequest(requestID, handRenderState, lens);
    const request = new CommunicationWrapper(ActionCode.SET_HAND_DATA_STREAM_STATE, content);

    const jsonContent = JSON.stringify(request);

    ConnectionManager.serviceConnection()?.SendMessage(jsonContent, requestID, () => {
        return true;
    });
};

const translateToCoordinate = (coordinate: Vector | undefined) => {
    if (coordinate === undefined) return new HandSvgCoordinate(-1, -1, -1);
    return new HandSvgCoordinate(1000 * (1 - coordinate.X * 1) - 100, 1000 * coordinate.Y - 100, coordinate.Z);
};

const tipJointIndex = 3;
const knuckleJointIndex = 1;

export const handToSvgData = (hand: RawHand, handIndex: number): HandSvgProps => {
    const indexFinger = hand.Fingers.find((f: RawFinger) => f.Type == 1);
    const middleFinger = hand.Fingers.find((f: RawFinger) => f.Type == 2);
    const ringFinger = hand.Fingers.find((f: RawFinger) => f.Type == 3);
    const littleFinger = hand.Fingers.find((f: RawFinger) => f.Type == 4);
    const thumbFinger = hand.Fingers.find((f: RawFinger) => f.Type == 0);
    const wrist = hand.WristPosition;
    return {
        indexTip: translateToCoordinate(indexFinger?.Bones[tipJointIndex]?.NextJoint),
        indexKnuckle: translateToCoordinate(indexFinger?.Bones[knuckleJointIndex]?.PrevJoint),
        middleTip: translateToCoordinate(middleFinger?.Bones[tipJointIndex]?.NextJoint),
        middleKnuckle: translateToCoordinate(middleFinger?.Bones[knuckleJointIndex]?.PrevJoint),
        ringTip: translateToCoordinate(ringFinger?.Bones[tipJointIndex]?.NextJoint),
        ringKnuckle: translateToCoordinate(ringFinger?.Bones[knuckleJointIndex]?.PrevJoint),
        littleTip: translateToCoordinate(littleFinger?.Bones[tipJointIndex]?.NextJoint),
        littleKnuckle: translateToCoordinate(littleFinger?.Bones[knuckleJointIndex]?.PrevJoint),
        thumbTip: translateToCoordinate(thumbFinger?.Bones[tipJointIndex]?.NextJoint),
        thumbKnuckle: translateToCoordinate(thumbFinger?.Bones[knuckleJointIndex]?.PrevJoint),
        wrist: translateToCoordinate(wrist),
        primaryHand: hand.CurrentPrimary,
        dotColor: handIndex ? 'blue' : 'red',
    };
};
