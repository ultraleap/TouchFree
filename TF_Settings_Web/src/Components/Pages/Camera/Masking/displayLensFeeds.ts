import { createBufferInfoFromArrays, createProgramInfo, setBuffersAndAttributes } from 'twgl.js';

let conversionArraysInitialised = false;
const byteConversionArray = new Uint32Array(256);
const byteConversionArrayOverExposed = new Uint32Array(256);

let cameraBuffer: ArrayBuffer;
let buf8: Uint8Array;
let buf32: Uint32Array;

const vs = `attribute vec4 position;
            varying vec2 v_texcoord;

            void main() {
                gl_Position = position;
                v_texcoord = position.xy * 0.5 + 0.5;
            }`;
const fs = `precision mediump float;
            uniform sampler2D u_texture;
            varying vec2 v_texcoord;

            void main() {
                gl_FragColor = texture2D(u_texture, v_texcoord);
            }`;

export const updateCanvas = (
    data: ArrayBuffer,
    gl: WebGLRenderingContext,
    isCameraReversed: boolean,
    showOverexposedAreas: boolean
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

    const width = 384;
    const lensHeight = 384;

    if (!cameraBuffer) {
        cameraBuffer = new ArrayBuffer(width * lensHeight);
        buf8 = new Uint8Array(cameraBuffer);
        buf32 = new Uint32Array(cameraBuffer);
    }

    processScreen(data, buf32, conversionArrayToUse, width, lensHeight);

    // Set black pixels to remove flashing camera bytes
    const startOffset = isCameraReversed ? 0 : ((lensHeight / 2 - 1) * width) / 2;
    buf32.fill(0xff000000, startOffset, startOffset + width);

    gl.texSubImage2D(gl.TEXTURE_2D, 0, 0, 0, gl.canvas.width, gl.canvas.height, gl.RGBA, gl.UNSIGNED_BYTE, buf8);
    gl.drawArrays(gl.TRIANGLES, 0, 6);
};

export const setupWebGL = (gl: WebGLRenderingContext) => {
    const texture = gl.createTexture();

    gl.bindTexture(gl.TEXTURE_2D, texture);
    gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, gl.canvas.width, gl.canvas.height, 0, gl.RGBA, gl.UNSIGNED_BYTE, null);

    gl.pixelStorei(gl.UNPACK_FLIP_Y_WEBGL, true);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.LINEAR);

    const programInfo = createProgramInfo(gl, [vs, fs]);
    const arrays = {
        position: [-1, -1, 0, 1, -1, 0, -1, 1, 0, -1, 1, 0, 1, -1, 0, 1, 1, 0],
    };
    const bufferInfo = createBufferInfoFromArrays(gl, arrays);

    gl.useProgram(programInfo.program);
    setBuffersAndAttributes(gl, programInfo, bufferInfo);
};

const processScreen = (
    data: ArrayBuffer,
    buf32: Uint32Array,
    byteConversionArray: Uint32Array,
    width: number,
    lensHeight: number
) => {
    const offsetView = new Uint8Array(data);

    for (let i = 0; i < width / 2; i++) {
        for (let j = 0; j < lensHeight / 2; j++) {
            buf32[(i * lensHeight) / 2 + j] = byteConversionArray[offsetView[i * lensHeight * 2 + j * 2]];
        }
    }
};
