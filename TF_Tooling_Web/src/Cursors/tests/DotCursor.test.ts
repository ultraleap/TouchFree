import TouchFree from '../../TouchFree';
import { InputType } from '../../TouchFreeToolingTypes';
import { mockTfInputAction } from '../../tests/testUtils';
import { DotCursor } from '../DotCursor';

const CURSOR_SIZE = 75;

TouchFree.Init({ initialiseCursor: false });

const cursor = document.createElement('img');
const cursorRing = document.createElement('img');

// Mocks required as Jest doesn't render anything meaning the client width/height of the elements is never set
jest.spyOn(cursor, 'clientWidth', 'get').mockImplementation(() => CURSOR_SIZE);
jest.spyOn(cursor, 'clientHeight', 'get').mockImplementation(() => CURSOR_SIZE);
jest.spyOn(cursorRing, 'clientWidth', 'get').mockImplementation(() => CURSOR_SIZE * 2);
jest.spyOn(cursorRing, 'clientHeight', 'get').mockImplementation(() => CURSOR_SIZE * 2);

cursor.classList.add('touchfree-cursor');
cursorRing.classList.add('touchfree-cursor');

document.body.appendChild(cursor);
document.body.appendChild(cursorRing);

const dotCursor = new DotCursor(cursor, cursorRing, 0);

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
        [0, 0.5, 1].forEach((progress) => {
            mockTfInputAction({ InputType: InputType.MOVE, ProgressToClick: progress });
            expect(cursorRing.style.opacity).toBe(progress.toString());
        });
    });

    test('Cursor should shrink when DOWN action received', () => {
        mockTfInputAction({ InputType: InputType.DOWN });
        expect(cursor.style.width).toBe('37.5px');
        expect(cursor.style.height).toBe('37.5px');
    });

    test('Cursor dot should return to original size when UP action received', () => {
        mockTfInputAction({ InputType: InputType.DOWN });
        mockTfInputAction({ InputType: InputType.UP });
        expect(cursor.style.width).toBe('75px');
        expect(cursor.style.height).toBe('75px');
    });

    test('HideCursor should prevent the cursor from being displayed', () => {
        dotCursor.HideCursor();
        expect(cursor.style.opacity).toBe('0');
        // Carry out TouchFree actions as these have an effect on the opacity of the cursor
        mockTfInputAction({ InputType: InputType.MOVE, CursorPosition: [100, 100] });
        expect(cursor.style.opacity).toBe('0');
        mockTfInputAction({ InputType: InputType.DOWN });
        expect(cursor.style.opacity).toBe('0');
        mockTfInputAction({ InputType: InputType.UP });
        expect(cursor.style.opacity).toBe('0');
    });

    test('ShowCursor should make the cursor visible when enabled', () => {
        dotCursor.HideCursor();
        expect(cursor.style.opacity).toBe('0');
        dotCursor.ShowCursor();
        expect(cursor.style.opacity).toBe('1');
        // Carry out TouchFree actions as these have an effect on the opacity of the cursor
        mockTfInputAction({ InputType: InputType.MOVE, CursorPosition: [100, 100] });
        expect(cursor.style.opacity).toBe('1');
        mockTfInputAction({ InputType: InputType.DOWN });
        expect(cursor.style.opacity).toBe('1');
        mockTfInputAction({ InputType: InputType.UP });
        expect(cursor.style.opacity).toBe('1');
    });

    test('DisableCursor should ensure the cursor cannot be visible', () => {
        dotCursor.DisableCursor();
        expect(cursor.style.opacity).toBe('0');
        dotCursor.ShowCursor();
        expect(cursor.style.opacity).toBe('0');
    });

    test('EnableCursor allows cursor to be shown again', async () => {
        dotCursor.DisableCursor();
        dotCursor.EnableCursor();
        dotCursor.ShowCursor();
        expect(cursor.style.opacity).toBe('1');
    });

    test('SetCursorOpacity sets the cursors opacity correctly', () => {
        for (let i = 0; i < 5; i++) {
            const randomNumber = Math.random();
            dotCursor.SetCursorOpacity(randomNumber);
            expect(cursor.style.opacity).toBe(randomNumber.toString());
        }
    });
});
