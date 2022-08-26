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

import { Lens } from './MaskingScreen';
import { FingerData, HandData, HandState } from './handRendering';

type BasicMesh = Mesh<BufferGeometry, MeshBasicMaterial>;

let conversionArraysInitialised = false;
const byteConversionArray = new Uint32Array(256);
const byteConversionArrayOverExposed = new Uint32Array(256);
let cameraBuffer: ArrayBuffer;

let _scene: Scene;
let _renderer: WebGLRenderer;
let _camera: PerspectiveCamera;
let _cameraFeedPlane: BasicMesh;
let _cameraTexture: DataTexture;

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

let _handOneMesh: HandMesh;
let _handTwoMesh: HandMesh;

export const updateCanvas = (
    data: ArrayBuffer,
    lens: Lens,
    isCameraReversed: boolean,
    showOverexposedAreas: boolean,
    handData?: HandState
) => {
    if (!conversionArraysInitialised) {
        for (let i = 0; i < 256; i++) {
            byteConversionArray[i] = (255 << 24) | (i << 16) | (i << 8) | i;
            // -13434625 = #FFFF0033 in signed 2's complement
            byteConversionArrayOverExposed[i] = i > 128 ? -13434625 : byteConversionArray[i];
        }
        conversionArraysInitialised = true;
    }

    const conversionArrayToUse = showOverexposedAreas ? byteConversionArrayOverExposed : byteConversionArray;

    const offset = 9;
    const startOfBuffer = new DataView(data, 1, offset);

    const dim1 = startOfBuffer.getUint32(0);
    const dim2 = startOfBuffer.getUint32(4);

    const width = Math.min(dim1, dim2);
    const lensHeight = Math.max(dim1, dim2) / 2;

    if (!cameraBuffer) {
        cameraBuffer = new ArrayBuffer(width * lensHeight * 4);
    }
    const buf8 = new Uint8Array(cameraBuffer);
    const buf32 = new Uint32Array(cameraBuffer);

    const rotated90 = dim2 < dim1;

    if (rotated90) {
        processRotatedScreen(data, lens, buf32, conversionArrayToUse, offset, width, lensHeight);
    } else {
        processScreen(data, lens, buf32, conversionArrayToUse, offset, width, lensHeight);
    }

    // Set black pixels to remove flashing camera bytes
    const startOffset = isCameraReversed ? 0 : (lensHeight - 1) * width;
    buf32.fill(0xff000000, startOffset, startOffset + width);

    _cameraTexture = new DataTexture(buf8, width, lensHeight);
    _cameraTexture.flipY = true;
    _cameraFeedPlane.material.color.setHex(0xffffff);
    _cameraFeedPlane.material.map = _cameraTexture;
    _cameraFeedPlane.material.needsUpdate = true;
    _cameraTexture.needsUpdate = true;

    updateHandMesh(_handOneMesh, handData?.one);
    updateHandMesh(_handTwoMesh, handData?.two);

    _renderer.render(_scene, _camera);
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

export const setupRenderScene = (div: HTMLDivElement) => {
    _scene = new Scene();
    _camera = new PerspectiveCamera(90);
    _camera.position.z = 2;

    _renderer = new WebGLRenderer();
    _renderer.setSize(div.clientWidth, div.clientHeight);
    div.appendChild(_renderer.domElement);

    _cameraFeedPlane = new Mesh(new PlaneGeometry(4, 4), new MeshBasicMaterial({ color: 0x000000 }));
    _scene.add(_cameraFeedPlane);

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

    _renderer.render(_scene, _camera);
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

const processRotatedScreen = (
    data: ArrayBuffer,
    lens: Lens,
    buf32: Uint32Array,
    byteConversionArray: Uint32Array,
    offset: number,
    width: number,
    lensHeight: number
) => {
    let rowBase = 0;
    const offsetView = new Uint8Array(data, offset, width * lensHeight * 2);

    for (let rowIndex = 0; rowIndex < width; rowIndex++) {
        let rowStart = rowBase * 2;

        if (lens === 'Right') {
            for (let i = 0; i < lensHeight; i++) {
                buf32[i + rowBase] = byteConversionArray[offsetView[i + rowStart]];
            }
        }

        rowStart += lensHeight;
        if (lens === 'Left') {
            for (let i = 0; i < lensHeight; i++) {
                buf32[i + rowBase] = byteConversionArray[offsetView[i + rowStart]];
            }
        }

        rowBase += lensHeight;
    }
};

const processScreen = (
    data: ArrayBuffer,
    lens: Lens,
    buf32: Uint32Array,
    byteConversionArray: Uint32Array,
    offset: number,
    width: number,
    lensHeight: number
) => {
    const offsetView = new Uint8Array(data, offset + (lens === 'Left' ? width * lensHeight : 0), width * lensHeight);

    for (let i = 0; i < width * lensHeight; i++) {
        buf32[i] = byteConversionArray[offsetView[i]];
    }
};
