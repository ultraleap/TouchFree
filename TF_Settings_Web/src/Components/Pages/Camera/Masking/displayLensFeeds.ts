import { createProgram, createProgramFromScripts, m4, resizeCanvasToDisplaySize, v3 } from 'twgl.js';

import { Lens } from './MaskingScreen';

let conversionArraysInitialised = false;
const byteConversionArray = new Uint32Array(256);
const byteConversionArrayOverExposed = new Uint32Array(256);
let cameraBuffer: ArrayBuffer;

export const createCanvasUpdate = (
    data: ArrayBuffer,
    gl: WebGLRenderingContext,
    lens: Lens,
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

    const offset = 9;
    const startOfBuffer = new DataView(data, 1, offset);

    const dim1 = startOfBuffer.getUint32(0);
    const dim2 = startOfBuffer.getUint32(4);

    const width = Math.min(dim1, dim2);
    const lensHeight = Math.max(dim1, dim2) / 2;

    if (!cameraBuffer) {
        cameraBuffer = new ArrayBuffer(width * lensHeight);
    }
    const buf8 = new Uint8ClampedArray(cameraBuffer);
    const buf32 = new Uint32Array(cameraBuffer);

    const rotated90 = dim2 < dim1;

    if (rotated90) {
        processRotatedScreen(data, lens, buf32, conversionArrayToUse, offset, width, lensHeight);
    } else {
        processScreen(data, lens, buf32, conversionArrayToUse, offset, width, lensHeight);
    }

    // Set black pixels to remove flashing camera bytes
    const startOffset = isCameraReversed ? 0 : ((lensHeight / 2 - 1) * width) / 2;
    buf32.fill(0xff000000, startOffset, startOffset + width);

    /*=========================Shaders========================*/
    // vertex shader source code
    const vertCode =
        'attribute vec4 a_position;' +
        'attribute vec2 a_texcoord;' +
        'uniform mat4 u_matrix;' +
        'varying vec2 v_texcoord;' +
        'void main() {' +
        ' gl_Position = u_matrix * a_position;' +
        ' v_texcoord = a_texcoord;' +
        '}';

    // fragment shader source code
    const fragCode =
        'precision mediump float;' +
        'varying vec2 v_texcoord;' +
        'uniform sampler2D u_texture;' +
        'varying vec2 v_texcoord;' +
        'void main() {' +
        ' gl_FragColor = texture2D(u_texture, v_texcoord);' +
        '}';

    // const program = createProgram(gl, [fragCode, vertCode]);
    const program = createProgramFromScripts(gl, ['drawImage-vertex-shader', 'drawImage-fragment-shader']);

    /*======== Associating shaders to buffer objects ========*/
    const positionLocation = gl.getAttribLocation(program, 'a_position');
    const texcoordLocation = gl.getAttribLocation(program, 'a_texcoord');
    const matrixLocation = gl.getUniformLocation(program, 'u_matrix');
    const textureLocation = gl.getUniformLocation(program, 'u_texture');

    const positionBuffer = gl.createBuffer();
    gl.bindBuffer(gl.ARRAY_BUFFER, positionBuffer);

    // Put a unit quad in the buffer
    const positions = [0, 0, 0, 1, 1, 0, 1, 0, 0, 1, 1, 1];
    gl.bufferData(gl.ARRAY_BUFFER, new Float32Array(positions), gl.STATIC_DRAW);

    // Create a buffer for texture coords
    const texcoordBuffer = gl.createBuffer();
    gl.bindBuffer(gl.ARRAY_BUFFER, texcoordBuffer);

    // Put texcoords in the buffer
    const texcoords = [0, 0, 0, 1, 1, 0, 1, 0, 0, 1, 1, 1];
    gl.bufferData(gl.ARRAY_BUFFER, new Float32Array(texcoords), gl.STATIC_DRAW);

    const tex = gl.createTexture();
    gl.bindTexture(gl.TEXTURE_2D, tex);
    // gl.texImage2D(
    //     gl.TEXTURE_2D,
    //     0,
    //     gl.RGBA,
    //     gl.canvas.width,
    //     gl.canvas.height,
    //     0,
    //     gl.RGBA,
    //     gl.UNSIGNED_BYTE,
    //     new Uint8Array([0, 0, 255, 255])
    // );

    // let's assume all images are not a power of 2
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.LINEAR);

    gl.bindTexture(gl.TEXTURE_2D, tex);
    console.log(width / 2, lensHeight / 2);
    gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, width / 2, lensHeight / 2, 0, gl.RGBA, gl.UNSIGNED_BYTE, buf8);

    resizeCanvasToDisplaySize(gl.canvas);
    gl.viewport(0, 0, gl.canvas.width, gl.canvas.height);

    gl.clear(gl.COLOR_BUFFER_BIT);

    gl.bindTexture(gl.TEXTURE_2D, tex);

    // Tell WebGL to use our shader program pair
    gl.useProgram(program);

    // Setup the attributes to pull data from our buffers
    gl.bindBuffer(gl.ARRAY_BUFFER, positionBuffer);
    gl.enableVertexAttribArray(positionLocation);
    gl.vertexAttribPointer(positionLocation, 2, gl.FLOAT, false, 0, 0);
    gl.bindBuffer(gl.ARRAY_BUFFER, texcoordBuffer);
    gl.enableVertexAttribArray(texcoordLocation);
    gl.vertexAttribPointer(texcoordLocation, 2, gl.FLOAT, false, 0, 0);

    // this matrix will convert from pixels to clip space
    let matrix = m4.ortho(0, gl.canvas.width, gl.canvas.height, 0, -1, 1);

    // // this matrix will scale our 1 unit quad
    // // from 1 unit to texWidth, texHeight units
    console.log(matrix);
    const scale = 800;
    matrix = m4.scale(matrix, [scale, scale, scale]);
    console.log(matrix);

    // // Set the matrix.
    gl.uniformMatrix4fv(matrixLocation, false, matrix);

    // Tell the shader to get the texture from texture unit 0
    gl.uniform1i(textureLocation, 0);

    // draw the quad (2 triangles, 6 vertices)
    gl.drawArrays(gl.TRIANGLES, 0, 6);

    // const texture = gl.createTexture();
    // const fb = gl.createFramebuffer();
    // gl.bindTexture(gl.TEXTURE_2D, texture);
    // gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, width / 2, lensHeight / 2, 0, gl.RGBA, gl.UNSIGNED_BYTE, buf8);

    // gl.bindFramebuffer(gl.FRAMEBUFFER, fb);
    // gl.framebufferTexture2D(gl.FRAMEBUFFER, gl.COLOR_ATTACHMENT0, gl.TEXTURE_2D, texture, 0);

    // context.readPixels(0, 0, width / 2, lensHeight / 2, context.RGBA, context.UNSIGNED_BYTE, buf8);
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
    const resultWidth = width / 2;
    const resultHeight = lensHeight / 2;

    for (let rowIndex = 0; rowIndex < resultWidth; rowIndex++) {
        const resultStartIndex = rowBase / 8;

        if (lens === 'Right') {
            for (let i = 0; i < resultHeight; i++) {
                buf32[i + resultStartIndex] = byteConversionArray[offsetView[i * 2 + rowBase]];
            }
        }

        rowBase += width;
        if (lens === 'Left') {
            for (let i = 0; i < resultHeight; i++) {
                buf32[i + resultStartIndex] = byteConversionArray[offsetView[i * 2 + rowBase]];
            }
        }

        rowBase += width;

        // Skip entire row
        rowBase += width * 2;
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

    for (let i = 0; i < width / 2; i++) {
        for (let j = 0; j < lensHeight / 2; j++) {
            buf32[(i * lensHeight) / 2 + j] = byteConversionArray[offsetView[i * lensHeight * 2 + j * 2]];
        }
    }
};
