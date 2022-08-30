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

import { FingerData, HandData, HandState } from './createHandData';

export type BasicMesh = Mesh<BufferGeometry, MeshBasicMaterial>;

interface FingerMesh {
    tip: BasicMesh;
    knuckle: BasicMesh;
    line: Line2;
}

interface HandMesh {
    fingers: {
        [FingerType.TYPE_THUMB]: FingerMesh;
        [FingerType.TYPE_INDEX]: FingerMesh;
        [FingerType.TYPE_MIDDLE]: FingerMesh;
        [FingerType.TYPE_RING]: FingerMesh;
        [FingerType.TYPE_PINKY]: FingerMesh;
    };
    wrist: BasicMesh;
}

let _scene: Scene;
let _renderer: WebGLRenderer;
let _camera: PerspectiveCamera;
let _cameraFeedMesh: BasicMesh;
let _cameraFeedTexture: DataTexture;

let _handOneMesh: HandMesh;
let _handTwoMesh: HandMesh;

export const setupRenderScene = (div: HTMLDivElement) => {
    _scene = new Scene();
    _camera = new PerspectiveCamera(90);
    _camera.position.z = 2;

    _renderer = new WebGLRenderer();
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
        wrist: addBasicCircleMesh(_scene),
    };

    _handTwoMesh = {
        fingers: {
            [FingerType.TYPE_THUMB]: addBasicFingerMesh(_scene),
            [FingerType.TYPE_INDEX]: addBasicFingerMesh(_scene),
            [FingerType.TYPE_MIDDLE]: addBasicFingerMesh(_scene),
            [FingerType.TYPE_RING]: addBasicFingerMesh(_scene),
            [FingerType.TYPE_PINKY]: addBasicFingerMesh(_scene),
        },
        wrist: addBasicCircleMesh(_scene),
    };
};

export const renderScene = () => _renderer.render(_scene, _camera);

export const updateCameraRender = (data: Uint8ClampedArray, width: number, height: number) => {
    if (!_cameraFeedTexture) {
        _cameraFeedTexture = new DataTexture(data, width, height);
        _cameraFeedTexture.flipY = true;
    } else {
        _cameraFeedTexture.image = new ImageData(data, width, height);
    }
    _cameraFeedTexture.needsUpdate = true;
    _cameraFeedMesh.material.map = _cameraFeedTexture;
    _cameraFeedMesh.material.needsUpdate = true;
};

export const updateHandRenders = (handData: HandState) => {
    updateHandMesh(_handOneMesh, handData?.one);
    updateHandMesh(_handTwoMesh, handData?.two);
};

const addBasicFingerMesh = (scene: Scene): FingerMesh => {
    return { tip: addBasicCircleMesh(scene), knuckle: addBasicCircleMesh(scene), line: addBasicLine(scene) };
};

const addBasicCircleMesh = (scene: Scene): BasicMesh => {
    const mesh = new Mesh(
        new CircleGeometry(0.05, 16),
        new MeshBasicMaterial({ color: cssVariables.ultraleapGreen, transparent: true })
    );
    mesh.visible = false;
    scene.add(mesh);
    return mesh;
};

const addBasicLine = (scene: Scene): Line2 => {
    const geometry = new LineGeometry();
    const line = new Line2(geometry, new LineMaterial({ color: 0xffffff, linewidth: 0.01, transparent: true }));
    line.visible = false;
    scene.add(line);
    return line;
};

const updateHandMesh = (handMesh: HandMesh, handData?: HandData) => {
    const isPrimary = handData?.primaryHand ?? false;
    Object.keys(handMesh.fingers).forEach((finger) => {
        const fingerType = Number(finger) as Exclude<FingerType, FingerType.TYPE_UNKNOWN>;
        updateFingerMesh(handMesh.fingers[fingerType], isPrimary, handData?.fingers[fingerType]);
    });

    if (handData) {
        moveMesh(handMesh.wrist, handData.wrist, isPrimary);
    } else {
        handMesh.wrist.visible = false;
    }
};

const updateFingerMesh = (fingerMesh: FingerMesh, isPrimary: boolean, finger?: FingerData) => {
    if (finger) {
        moveMesh(fingerMesh.tip, finger.tip, isPrimary);
        moveMesh(fingerMesh.knuckle, finger.knuckle, isPrimary);
        moveLine(fingerMesh.line, finger.knuckle, finger.tip, isPrimary);
    } else {
        fingerMesh.tip.visible = false;
        fingerMesh.knuckle.visible = false;
        fingerMesh.line.visible = false;
    }
};

const moveLine = (line: Line2, start: Vector3, end: Vector3, isPrimary: boolean) => {
    line.geometry.setPositions([start.x, start.y, start.z + 0.01, end.x, end.y, end.z + 0.01]);
    line.material.opacity = isPrimary ? 1 : 0.5;
    line.material.needsUpdate = true;
    line.visible = true;
};

const moveMesh = (mesh: BasicMesh, position: Vector3, isPrimary: boolean) => {
    mesh.position.set(position.x, position.y, position.z);
    mesh.material.opacity = isPrimary ? 1 : 0.5;
    mesh.material.needsUpdate = true;
    mesh.visible = true;
    // const scale = 1 + 2 * position.z;
    // mesh.scale.set(scale, scale, 1);
};
