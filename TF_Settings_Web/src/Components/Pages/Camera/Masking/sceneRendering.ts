import cssVariables from 'Styles/_variables.scss';

import {
    BufferGeometry,
    CircleGeometry,
    DataTexture,
    Mesh,
    MeshBasicMaterial,
    PerspectiveCamera,
    PlaneGeometry,
    Scene,
    Vector3,
    WebGLRenderer,
} from 'three';
import { Line2 } from 'three/examples/jsm/lines/Line2.js';
import { LineGeometry } from 'three/examples/jsm/lines/LineGeometry';
import { LineMaterial } from 'three/examples/jsm/lines/LineMaterial';

import { FingerType } from 'TouchFree/TouchFreeToolingTypes';
import { MapRangeToRange } from 'TouchFree/Utilities';

import { FingerData, HandData, HandState } from './createHandData';

export type BasicMesh = Mesh<BufferGeometry, MeshBasicMaterial>;

interface FingerMesh {
    tip: BasicMesh;
    knuckle: BasicMesh;
    lineToTip: Line2;
    lineToNextKnuckle: Line2;
}

interface WristMesh {
    point: BasicMesh;
    lineToThumb: Line2;
}

interface HandMesh {
    fingers: {
        [FingerType.TYPE_THUMB]: FingerMesh;
        [FingerType.TYPE_INDEX]: FingerMesh;
        [FingerType.TYPE_MIDDLE]: FingerMesh;
        [FingerType.TYPE_RING]: FingerMesh;
        [FingerType.TYPE_PINKY]: FingerMesh;
    };
    wrist: WristMesh;
}

let _scene: Scene;
let _renderer: WebGLRenderer;
let _camera: PerspectiveCamera;
let _cameraFeedMesh: BasicMesh;
let _cameraFeedTexture: DataTexture;

let _handOneMesh: HandMesh;
let _handTwoMesh: HandMesh;

const BASE_LINE_THICKNESS = 0.005;

export const setupRenderScene = (div: HTMLDivElement) => {
    _scene = new Scene();
    _camera = new PerspectiveCamera(90);
    _camera.position.z = 2;

    _renderer = new WebGLRenderer();
    _renderer.setPixelRatio(window.devicePixelRatio * 0.35);
    _renderer.setSize(div.clientWidth, div.clientHeight);
    div.appendChild(_renderer.domElement);

    _cameraFeedMesh = new Mesh(new PlaneGeometry(4, 4), new MeshBasicMaterial());
    _scene.add(_cameraFeedMesh);

    _handOneMesh = {
        fingers: {
            [FingerType.TYPE_THUMB]: addBasicFingerMesh(_scene),
            [FingerType.TYPE_INDEX]: addBasicFingerMesh(_scene),
            [FingerType.TYPE_MIDDLE]: addBasicFingerMesh(_scene),
            [FingerType.TYPE_RING]: addBasicFingerMesh(_scene),
            [FingerType.TYPE_PINKY]: addBasicFingerMesh(_scene),
        },
        wrist: addBasicWristMesh(_scene),
    };

    _handTwoMesh = {
        fingers: {
            [FingerType.TYPE_THUMB]: addBasicFingerMesh(_scene),
            [FingerType.TYPE_INDEX]: addBasicFingerMesh(_scene),
            [FingerType.TYPE_MIDDLE]: addBasicFingerMesh(_scene),
            [FingerType.TYPE_RING]: addBasicFingerMesh(_scene),
            [FingerType.TYPE_PINKY]: addBasicFingerMesh(_scene),
        },
        wrist: addBasicWristMesh(_scene),
    };
};

export const renderScene = () => _renderer.render(_scene, _camera);

export const updateCameraRender = (data: Uint8Array, width: number, height: number) => {
    if (_cameraFeedTexture) _cameraFeedTexture.dispose();

    _cameraFeedTexture = new DataTexture(data, width, height);
    _cameraFeedTexture.flipY = true;
    _cameraFeedTexture.needsUpdate = true;

    _cameraFeedMesh.material.map = _cameraFeedTexture;
    _cameraFeedMesh.material.needsUpdate = true;

    console.log(_renderer.info);
};

export const updateHandRenders = (handData: HandState) => {
    updateHandMesh(_handOneMesh, handData?.one);
    updateHandMesh(_handTwoMesh, handData?.two);
};

const addBasicFingerMesh = (scene: Scene): FingerMesh => ({
    tip: addBasicCircleMesh(scene),
    knuckle: addBasicCircleMesh(scene),
    lineToTip: addBasicLine(scene),
    lineToNextKnuckle: addBasicLine(scene),
});

const addBasicWristMesh = (scene: Scene): WristMesh => ({
    point: addBasicCircleMesh(scene),
    lineToThumb: addBasicLine(scene),
});

const addBasicCircleMesh = (scene: Scene): BasicMesh => {
    const mesh = new Mesh(
        new CircleGeometry(0.02, 16),
        new MeshBasicMaterial({ color: cssVariables.ultraleapGreen, transparent: true })
    );
    mesh.visible = false;
    scene.add(mesh);
    return mesh;
};

const addBasicLine = (scene: Scene): Line2 => {
    const geometry = new LineGeometry();
    const line = new Line2(
        geometry,
        new LineMaterial({ color: 0xffffff, linewidth: BASE_LINE_THICKNESS, transparent: true })
    );
    line.visible = false;
    scene.add(line);
    return line;
};

const updateHandMesh = (handMesh: HandMesh, handData?: HandData) => {
    const isPrimary = handData?.primaryHand ?? false;
    Object.keys(handMesh.fingers).forEach((finger) => {
        const fingerType = Number(finger) as Exclude<FingerType, FingerType.TYPE_UNKNOWN>;
        let nextKnucklePos: Vector3 | undefined;
        if (fingerType === FingerType.TYPE_PINKY) {
            nextKnucklePos = handData?.wrist;
        } else {
            nextKnucklePos =
                handData?.fingers[(fingerType + 1) as Exclude<FingerType, FingerType.TYPE_UNKNOWN>].knuckle;
        }
        updateFingerMesh(handMesh.fingers[fingerType], isPrimary, handData?.fingers[fingerType], nextKnucklePos);
    });

    updateWristMesh(handMesh.wrist, isPrimary, handData?.wrist, handData?.fingers[FingerType.TYPE_THUMB].knuckle);
};

const updateFingerMesh = (
    fingerMesh: FingerMesh,
    isPrimary: boolean,
    finger?: FingerData,
    nextKnucklePos?: Vector3
) => {
    if (finger && nextKnucklePos) {
        moveMesh(fingerMesh.tip, finger.tip, isPrimary);
        moveMesh(fingerMesh.knuckle, finger.knuckle, isPrimary);
        moveLine(fingerMesh.lineToTip, finger.knuckle, finger.tip, isPrimary);
        moveLine(fingerMesh.lineToNextKnuckle, finger.knuckle, nextKnucklePos, isPrimary);
    } else {
        fingerMesh.tip.visible = false;
        fingerMesh.knuckle.visible = false;
        fingerMesh.lineToTip.visible = false;
        fingerMesh.lineToNextKnuckle.visible = false;
    }
};

const updateWristMesh = (wristMesh: WristMesh, isPrimary: boolean, wrist?: Vector3, thumbKnucklePos?: Vector3) => {
    if (wrist && thumbKnucklePos) {
        moveMesh(wristMesh.point, wrist, isPrimary);
        moveLine(wristMesh.lineToThumb, wrist, thumbKnucklePos, isPrimary);
    } else {
        wristMesh.point.visible = false;
        wristMesh.lineToThumb.visible = false;
    }
};

const moveLine = (line: Line2, start: Vector3, end: Vector3, isPrimary: boolean) => {
    line.geometry.setPositions([start.x, start.y, start.z, end.x, end.y, end.z]);
    const scale = MapRangeToRange((start.z + end.z) / 2, 0, 0.1, 1, 3);
    line.material.linewidth = BASE_LINE_THICKNESS * scale;
    line.material.opacity = isPrimary ? 1 : 0.5;
    line.material.needsUpdate = true;
    line.visible = true;
};

const moveMesh = (mesh: BasicMesh, position: Vector3, isPrimary: boolean) => {
    mesh.position.set(position.x, position.y, position.z + 0.0001);
    mesh.material.opacity = isPrimary ? 1 : 0.5;
    mesh.material.needsUpdate = true;
    mesh.visible = true;

    const scale = MapRangeToRange(position.z, 0, 0.1, 1, 3);
    mesh.scale.set(scale, scale, 1);
};
