import { Vector3 } from 'three';

import { v4 as uuidgen } from 'uuid';

import { Vector } from 'TouchFree/Configuration/ConfigurationTypes';
import { ConnectionManager } from 'TouchFree/Connection/ConnectionManager';
import {
    ActionCode,
    CommunicationWrapper,
    HandRenderDataStateRequest,
} from 'TouchFree/Connection/TouchFreeServiceTypes';
import { FingerType, RawFinger, RawHand } from 'TouchFree/TouchFreeToolingTypes';
import { MapRangeToRange } from 'TouchFree/Utilities';

export interface FingerData {
    tip: Vector3;
    knuckle: Vector3;
}
export interface HandData {
    fingers: {
        [FingerType.TYPE_THUMB]: FingerData;
        [FingerType.TYPE_INDEX]: FingerData;
        [FingerType.TYPE_MIDDLE]: FingerData;
        [FingerType.TYPE_RING]: FingerData;
        [FingerType.TYPE_PINKY]: FingerData;
    };
    wrist: Vector3;
    primaryHand: boolean;
}

export class HandState {
    one?: HandData;
    two?: HandData;
}

export const setHandRenderState = (handRenderState: boolean, lens: 'left' | 'right'): void => {
    const requestID = uuidgen();

    const content = new HandRenderDataStateRequest(requestID, handRenderState, lens);
    const request = new CommunicationWrapper(ActionCode.SET_HAND_DATA_STREAM_STATE, content);

    const jsonContent = JSON.stringify(request);

    ConnectionManager.serviceConnection()?.SendMessage(jsonContent, requestID, () => {
        return true;
    });
};

export const rawHandToHandData = (hand: RawHand): HandData => {
    const fingersData = hand.Fingers;

    return {
        fingers: {
            [FingerType.TYPE_THUMB]: createFingerData(fingersData, FingerType.TYPE_THUMB),
            [FingerType.TYPE_INDEX]: createFingerData(fingersData, FingerType.TYPE_INDEX),
            [FingerType.TYPE_MIDDLE]: createFingerData(fingersData, FingerType.TYPE_MIDDLE),
            [FingerType.TYPE_RING]: createFingerData(fingersData, FingerType.TYPE_RING),
            [FingerType.TYPE_PINKY]: createFingerData(fingersData, FingerType.TYPE_PINKY),
        },
        wrist: translateToCoordinate(hand.WristPosition),
        primaryHand: hand.CurrentPrimary,
    };
};

const tipJointIndex = 3;
const knuckleJointIndex = 1;

const createFingerData = (fingers: RawFinger[], fingerType: number): FingerData => {
    const finger = fingers.find((f: RawFinger) => f.Type == fingerType);
    return {
        tip: translateToCoordinate(finger?.Bones[tipJointIndex]?.NextJoint),
        knuckle: translateToCoordinate(finger?.Bones[knuckleJointIndex]?.PrevJoint),
    };
};

const translateToCoordinate = (coordinate?: Vector) => {
    if (coordinate === undefined) return new Vector3(-1, -1, -1);
    const { X, Y } = coordinate;
    // Map Z between 0 and 1
    // const mappedZ = Z > 600 ? 0 : 1 - MapRangeToRange(Z, 0, 600, 0, 1);
    const mappedZ = 0;
    return new Vector3(1.2 * MapRangeToRange(1 - X, 0, 1, -2, 2), 1 * MapRangeToRange(1 - Y, 0, 1, -2, 2), mappedZ);
};
