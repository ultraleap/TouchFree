import TouchFree from '../../TouchFree';
import { InputType } from '../../TouchFreeToolingTypes';
import { mockTfInputAction } from '../../tests/testUtils';
import { DotCursor } from '../DotCursor';

TouchFree.Init({ initialiseCursor: false });
const cursor = document.createElement('img');
const cursorRing = document.createElement('img');

cursor.src = './images/Dot.png';
cursor.style.position = 'absolute';
cursor.width = 75;
cursor.height = 75;
cursor.style.zIndex = '1001';
cursor.classList.add('touchfreecursor');
cursorRing.classList.add('touchfreecursor');

const dotCursor = new DotCursor(cursor, cursorRing);

describe('Dot Cursor', () => {
    test('Update cursor position when MOVE action received', () => {
        mockTfInputAction({ InputType: InputType.MOVE, CursorPosition: [100, 100] });
        expect(cursor?.style.left).toBe('100px');
        expect(cursorRing?.style.top).toBe('100px');
    });

    test('Cursor ring should grow with ProgressToClick', () => {
        expect(cursorRing?.style.width).toBe('30px');

        mockTfInputAction({ InputType: InputType.MOVE, ProgressToClick: 0.5 });

        expect(cursorRing?.style.width).toBe('23px');
    });

    test('Cursor ring should fade in with ProgressToClick', () => {
        [0, 0.5, 1].forEach((progress) => {
            mockTfInputAction({ InputType: InputType.MOVE, ProgressToClick: progress });
            expect(cursorRing?.getAttribute('opacity')).toBe(progress.toString());
        });
    });

    test('Cursor should shrink when DOWN action received', () => {
        mockTfInputAction({ InputType: InputType.DOWN });
        expect(cursor?.style.transform).toBe('scale(0.5)');
        expect(cursorRing?.getAttribute('r')).toBe('0');
    });

    test('Cursor dot should return to original size when UP action received', () => {
        mockTfInputAction({ InputType: InputType.DOWN });
        expect(cursor?.style.transform).toBe('scale(0.5)');
        mockTfInputAction({ InputType: InputType.UP });
        expect(cursor?.style.transform).toBe('scale(1)');
    });

    test('HideCursor should prevent the cursor from being displayed', () => {
        dotCursor.HideCursor();
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
        expect(cursor?.style.transform).toBe('scale(0.5)');
        dotCursor.HideCursor();
        expect(cursor?.style.transform).toBe('scale(1)');
    });

    test('ShowCursor should make the cursor visible', () => {
        dotCursor.HideCursor();
        expect(cursor?.style.opacity).toBe('0');
        dotCursor.ShowCursor();
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
        dotCursor.DisableCursor();
        expect(cursor?.style.opacity).toBe('0');
        dotCursor.ShowCursor();
        expect(cursor?.style.opacity).toBe('0');
    });

    test('EnableCursor allows cursor to be shown again', () => {
        dotCursor.DisableCursor();
        console.log(dotCursor.shouldShow);
        dotCursor.EnableCursor();
        expect(cursor?.style.opacity).toBe('1');
    });

    test('SetCursorOpacity sets the cursors opacity correctly', () => {
        for (let i = 0; i < 5; i++) {
            const randomNumber = Math.random();
            dotCursor.SetCursorOpacity(randomNumber);
            expect(cursor?.style.opacity).toBe(randomNumber.toString());
        }
    });

    test('ShowCursor should set opacity to the same as before hands lost', () => {
        dotCursor.SetCursorOpacity(0.4);
        dotCursor.HideCursor();
        setTimeout(() => {
            expect(cursor?.style.opacity).toBe('0');
            dotCursor.ShowCursor();
            expect(cursor?.style.opacity).toBe('0.4');
        }, dotCursor.animationUpdateDuration);
    });
});
