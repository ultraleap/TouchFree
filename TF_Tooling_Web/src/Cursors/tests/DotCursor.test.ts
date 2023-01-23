import TouchFree from '../../TouchFree';
import { InputType } from '../../TouchFreeToolingTypes';
import { mockTfInputAction } from '../../tests/testUtils';
import { DotCursor } from '../DotCursor';

const CURSOR_SIZE = 75;
const CURSOR_SIZE_STRING = `${CURSOR_SIZE}px`;
const CURSOR_RING_SIZE_STRING = `${CURSOR_SIZE * 2}px`;

TouchFree.Init({ initialiseCursor: false });

const cursor = document.createElement('img');
const cursorRing = document.createElement('img');

cursor.style.width = CURSOR_SIZE_STRING;
cursor.style.height = CURSOR_SIZE_STRING;
cursorRing.style.width = CURSOR_RING_SIZE_STRING;
cursorRing.style.height = CURSOR_RING_SIZE_STRING;

cursor.classList.add('touchfree-cursor');
cursorRing.classList.add('touchfree-cursor');

document.body.appendChild(cursor);
document.body.appendChild(cursorRing);

const dotCursor = new DotCursor(cursor, cursorRing);

jest.setTimeout(10000);

describe('Dot Cursor', () => {
    beforeAll(() => {
        // Set cursor to known state before each test
        mockTfInputAction();
        mockTfInputAction({ InputType: InputType.UP });
        dotCursor.EnableCursor();
        dotCursor.ShowCursor();
    });

    test('Update cursor position when MOVE action received', () => {
        mockTfInputAction({ InputType: InputType.MOVE, CursorPosition: [100, 100] });
        const pos = `${100 - CURSOR_SIZE / 2}px`;
        const ringPos = `${100 - CURSOR_SIZE}px`;
        expect(cursor.style.left).toBe(pos);
        expect(cursor.style.top).toBe(pos);
        expect(cursorRing.style.left).toBe(ringPos);
        expect(cursorRing.style.top).toBe(ringPos);
    });

    test('Cursor ring should shrink with ProgressToClick', () => {
        expect(cursorRing.style.width).toBe('150px');
        mockTfInputAction({ InputType: InputType.MOVE, ProgressToClick: 0.5 });
        expect(cursorRing.style.width).toBe('112.5px');
    });

    test('Cursor ring should fade in with ProgressToClick', () => {
        for (const progress of [0, 0.5, 1]) {
            mockTfInputAction({ InputType: InputType.MOVE, ProgressToClick: progress });
            expect(cursorRing.style.opacity).toBe(progress.toString());
        }
    });

    test('Cursor should shrink when DOWN action received', async () => {
        mockTfInputAction({ InputType: InputType.DOWN });
        expect(cursorRing.style.width).toBe('0px');
        expect(cursorRing.style.height).toBe('0px');
        await testCursorStyle('width', '37.5px');
        await testCursorStyle('height', '37.5px');
    });

    test('Cursor dot should return to original size when UP action received', async () => {
        mockTfInputAction({ InputType: InputType.DOWN });
        mockTfInputAction({ InputType: InputType.UP });
        await testCursorStyle('width', '75px');
        await testCursorStyle('height', '75px');
    });

    test('HideCursor should prevent the cursor from being displayed', async () => {
        dotCursor.HideCursor();
        await testCursorStyle('opacity', '0');
        // Carry out TouchFree actions as these have an effect on the opacity of the cursor
        mockTfInputAction({ InputType: InputType.MOVE, CursorPosition: [100, 100] });
        await testCursorStyle('opacity', '0');
        mockTfInputAction({ InputType: InputType.DOWN });
        await testCursorStyle('opacity', '0');
        mockTfInputAction({ InputType: InputType.UP });
        await testCursorStyle('opacity', '0');
    });

    test('ShowCursor should make the cursor visible when enabled', async () => {
        dotCursor.HideCursor();
        await testCursorStyle('opacity', '0');
        dotCursor.ShowCursor();
        await testCursorStyle('opacity', '1');
        // Carry out TouchFree actions as these have an effect on the opacity of the cursor
        mockTfInputAction({ InputType: InputType.MOVE, CursorPosition: [100, 100] });
        await testCursorStyle('opacity', '1');
        mockTfInputAction({ InputType: InputType.DOWN });
        await testCursorStyle('opacity', '1');
        mockTfInputAction({ InputType: InputType.UP });
        await testCursorStyle('opacity', '1');
    });

    test('DisableCursor should ensure the cursor cannot be visible', async () => {
        dotCursor.DisableCursor();
        await testCursorStyle('opacity', '0');
        dotCursor.ShowCursor();
        await testCursorStyle('opacity', '0');
    });

    test('EnableCursor allows cursor to be shown again', async () => {
        dotCursor.DisableCursor();
        dotCursor.EnableCursor();
        dotCursor.ShowCursor();
        await testCursorStyle('opacity', '1');
    });

    test('SetCursorOpacity sets the cursors opacity correctly', () => {
        for (let i = 0; i < 5; i++) {
            const randomNumber = Math.random();
            dotCursor.SetCursorOpacity(randomNumber);
            expect(cursor.style.opacity).toBe(randomNumber.toString());
        }
    });
});

const getStyle = (key: keyof CSSStyleDeclaration) => cursor.style[key];

const testCursorStyle = async (key: keyof CSSStyleDeclaration, expected: string) => {
    await new Promise<void>((resolve, reject) => {
        setTimeout(() => {
            try {
                expect(getStyle(key)).toBe(expected);
                resolve();
            } catch (e) {
                reject(e);
            }
        }, 1000);
    });
};
