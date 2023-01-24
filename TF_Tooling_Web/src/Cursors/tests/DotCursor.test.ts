import TouchFree from '../../TouchFree';
import { InputType } from '../../TouchFreeToolingTypes';
import { mockTfInputAction } from '../../tests/testUtils';
import { DotCursor } from '../DotCursor';

const CURSOR_SIZE = 75;

TouchFree.Init({ initialiseCursor: false });
let dotCursor: DotCursor;
let cursor: HTMLImageElement;
let cursorRing: HTMLImageElement;

const setup = (ringSizeMultiplier?: number) => {
    cursor = document.createElement('img');
    cursorRing = document.createElement('img');

    cursor.style.width = cursor.style.height = `${CURSOR_SIZE}px`;

    cursor.classList.add('touchfree-cursor');
    cursorRing.classList.add('touchfree-cursor');

    document.body.appendChild(cursor);
    document.body.appendChild(cursorRing);
    if (ringSizeMultiplier) {
        dotCursor = new DotCursor(cursor, cursorRing, 0.2, ringSizeMultiplier);
    } else {
        dotCursor = new DotCursor(cursor, cursorRing);
    }
    mockTfInputAction();
};

describe('Dot Cursor', () => {
    beforeEach(() => setup());

    afterEach(() => {
        document.body.removeChild(cursor);
        document.body.removeChild(cursorRing);
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
        // Test for different ringSizeMultiplier
        setup(4);
        expect(cursorRing.style.width).toBe('300px');
        mockTfInputAction({ InputType: InputType.MOVE, ProgressToClick: 0.5 });
        expect(cursorRing.style.width).toBe('187.5px');
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
        let time = 0;
        const interval = setInterval(() => {
            try {
                expect(getStyle(key)).toBe(expected);
                clearInterval(interval);
                resolve();
            } catch (e) {
                if (time > 1000) {
                    clearInterval(interval);
                    reject(e);
                }
                time += 20;
            }
        }, 20);
    });
};
