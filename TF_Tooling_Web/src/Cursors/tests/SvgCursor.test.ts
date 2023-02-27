import TouchFree from '../../TouchFree';
import { InputType } from '../../TouchFreeToolingTypes';
import { mockTfInputAction } from '../../tests/testUtils';
import { CursorPart, SVGCursor } from '../SvgCursor';

TouchFree.Init();
let svgCursor = new SVGCursor();
let cursor = document.getElementById('svg-cursor');
let cursorRing = document.getElementById('svg-cursor-ring');
let cursorDot = document.getElementById('svg-cursor-dot');

const checkDefaultCursorColors = (isDarkCursor = false) => {
    const baseColor = isDarkCursor ? 'black' : 'white';
    expect(cursorDot?.getAttribute('fill')).toBe(baseColor);
    expect(cursorDot?.getAttribute('stroke')).toBe(null);
    expect(cursorDot?.getAttribute('stroke-width')).toBe(null);
    expect(cursorRing?.getAttribute('stroke')).toBe(baseColor);
};

const setNonDefaultCursorColors = () => {
    svgCursor.SetColor(CursorPart.CENTER_FILL, 'red');
    svgCursor.SetColor(CursorPart.RING_FILL, 'blue');
    svgCursor.SetColor(CursorPart.CENTER_BORDER, 'green');
    expect(cursorDot?.getAttribute('fill')).toBe('red');
    expect(cursorDot?.getAttribute('stroke')).toBe('green');
    expect(cursorDot?.getAttribute('stroke-width')).toBe('2');
    expect(cursorRing?.getAttribute('stroke')).toBe('blue');
};

describe('SVG Cursor', () => {
    beforeEach(() => {
        // Set cursor to known state before each test
        mockTfInputAction();
        mockTfInputAction({ InputType: InputType.UP });
        svgCursor.EnableCursor();
        svgCursor.ShowCursor();
        svgCursor.ResetToDefaultColors();
    });

    test('Creates a cursor in the document body', () => {
        expect(cursor).toBeTruthy();
        expect(cursorDot).toBeTruthy();
        expect(cursorRing).toBeTruthy();
    });

    test('Update cursor position when MOVE action received', () => {
        mockTfInputAction({ InputType: InputType.MOVE, CursorPosition: [100, 100] });
        expect(cursorDot?.getAttribute('cx')).toBe('100');
        expect(cursorRing?.getAttribute('cy')).toBe('100');
    });

    test('Cursor ring should shrink with ProgressToClick', () => {
        expect(cursorRing?.getAttribute('r')).toBe('30');

        mockTfInputAction({ InputType: InputType.MOVE, ProgressToClick: 0.5 });

        expect(cursorRing?.getAttribute('r')).toBe('23');
    });

    test('Cursor ring should fade in with ProgressToClick', () => {
        [0, 0.5, 1].forEach((progress) => {
            mockTfInputAction({ InputType: InputType.MOVE, ProgressToClick: progress });
            expect(cursorRing?.getAttribute('opacity')).toBe(progress.toString());
        });
    });

    test('Cursor should shrink when DOWN action received', () => {
        mockTfInputAction({ InputType: InputType.DOWN });
        expect(cursorDot?.style.transform).toBe('scale(0.5)');
        expect(cursorRing?.getAttribute('r')).toBe('0');
    });

    test('Cursor dot should return to original size when UP action received', () => {
        mockTfInputAction({ InputType: InputType.DOWN });
        expect(cursorDot?.style.transform).toBe('scale(0.5)');
        mockTfInputAction({ InputType: InputType.UP });
        expect(cursorDot?.style.transform).toBe('scale(1)');
    });

    test('HideCursor should prevent the cursor from being displayed', () => {
        svgCursor.HideCursor();
        expect(cursor?.style.opacity).toBe('0');
        // Carry out TouchFree actions as these have an effect on the opacity of the cursor
        mockTfInputAction({ InputType: InputType.MOVE, CursorPosition: [100, 100] });
        expect(cursor?.style.opacity).toBe('0');
        mockTfInputAction({ InputType: InputType.DOWN });
        expect(cursor?.style.opacity).toBe('0');
        mockTfInputAction({ InputType: InputType.UP });
        expect(cursor?.style.opacity).toBe('0');
    });

    test('HideCursor should return the scale of the cursor dot to 1', () => {
        mockTfInputAction({ InputType: InputType.DOWN });
        expect(cursorDot?.style.transform).toBe('scale(0.5)');
        svgCursor.HideCursor();
        expect(cursorDot?.style.transform).toBe('scale(1)');
    });

    test('ShowCursor should make the cursor visible', () => {
        svgCursor.HideCursor();
        expect(cursor?.style.opacity).toBe('0');
        svgCursor.ShowCursor();
        expect(cursor?.style.opacity).toBe('1');
        // Carry out TouchFree actions as these have an effect on the opacity of the cursor
        mockTfInputAction({ InputType: InputType.MOVE, CursorPosition: [100, 100] });
        expect(cursor?.style.opacity).toBe('1');
        mockTfInputAction({ InputType: InputType.DOWN });
        expect(cursor?.style.opacity).toBe('1');
        mockTfInputAction({ InputType: InputType.UP });
        expect(cursor?.style.opacity).toBe('1');
    });

    test('DisableCursor should ensure the cursor cannot be visible', () => {
        svgCursor.DisableCursor();
        expect(cursor?.style.opacity).toBe('0');
        svgCursor.ShowCursor();
        expect(cursor?.style.opacity).toBe('0');
    });

    test('EnableCursor allows cursor to be shown again', () => {
        svgCursor.DisableCursor();
        svgCursor.EnableCursor();
        expect(cursor?.style.opacity).toBe('1');
    });

    test('SetCursorOpacity sets the cursors opacity correctly', () => {
        for (let i = 0; i < 5; i++) {
            const randomNumber = Math.random();
            svgCursor.SetCursorOpacity(randomNumber);
            expect(cursor?.style.opacity).toBe(randomNumber.toString());
        }
    });

    test('ShowCursor should set opacity to the same as before hands lost', () => {
        svgCursor.SetCursorOpacity(0.4);
        svgCursor.HideCursor();
        expect(cursor?.style.opacity).toBe('0');
        svgCursor.ShowCursor();
        expect(cursor?.style.opacity).toBe('0.4');
    });

    test('SetColor should set the color of the correct cursor part', () => {
        checkDefaultCursorColors();

        svgCursor.SetColor(CursorPart.CENTER_FILL, 'red');
        expect(cursorDot?.getAttribute('fill')).toBe('red');
        expect(cursorDot?.getAttribute('stroke')).toBe(null);
        expect(cursorDot?.getAttribute('stroke-width')).toBe(null);
        expect(cursorRing?.getAttribute('stroke')).toBe('white');

        svgCursor.SetColor(CursorPart.RING_FILL, 'blue');
        expect(cursorDot?.getAttribute('fill')).toBe('red');
        expect(cursorDot?.getAttribute('stroke')).toBe(null);
        expect(cursorDot?.getAttribute('stroke-width')).toBe(null);
        expect(cursorRing?.getAttribute('stroke')).toBe('blue');

        svgCursor.SetColor(CursorPart.CENTER_BORDER, 'green');
        expect(cursorDot?.getAttribute('fill')).toBe('red');
        expect(cursorDot?.getAttribute('stroke')).toBe('green');
        expect(cursorDot?.getAttribute('stroke-width')).toBe('2');
        expect(cursorRing?.getAttribute('stroke')).toBe('blue');
    });

    test('ResetToDefaultColors should reset the cursor colors', () => {
        checkDefaultCursorColors();
        setNonDefaultCursorColors();

        svgCursor.ResetToDefaultColors();
        checkDefaultCursorColors();
    });

    describe('SVG Cursor darkCursor', () => {
        beforeAll(() => {
            cursor?.remove();
            cursorDot?.remove();
            cursorRing?.remove();

            svgCursor = new SVGCursor(2, true);
            cursor = document.getElementById('svg-cursor');
            cursorRing = document.getElementById('svg-cursor-ring');
            cursorDot = document.getElementById('svg-cursor-dot');
        });

        test('Cursor has the correct colors with darkCursor set', () => {
            checkDefaultCursorColors(true);
        });

        test('ResetToDefaultColors should reset the cursor colors', () => {
            checkDefaultCursorColors(true);
            setNonDefaultCursorColors();

            svgCursor.ResetToDefaultColors();
            checkDefaultCursorColors(true);
        });
    });
});
