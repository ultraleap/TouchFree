import TouchFree from '../../TouchFree';
import { HandChirality, HandType, InputType, InteractionType, TouchFreeInputAction } from '../../TouchFreeToolingTypes';
import { SVGCursor } from '../SvgCursor';

TouchFree.Init();

new SVGCursor();
const cursor = document.querySelector('#svg-cursor');
const cursorRing = document.querySelector('#svg-cursor-ring');
const cursorDot = document.querySelector('#svg-cursor-dot');
const mockTfInputAction = (input: Partial<TouchFreeInputAction>) =>
    new TouchFreeInputAction(
        input.Timestamp ?? Date.now(),
        input.InteractionType ?? InteractionType.PUSH,
        input.HandType ?? HandType.PRIMARY,
        input.Chirality ?? HandChirality.RIGHT,
        input.InputType ?? InputType.MOVE,
        input.CursorPosition ?? [0, 0],
        input.DistanceFromScreen ?? 5,
        input.ProgressToClick ?? 0
    );

describe('SVG Cursor', () => {
    test('Creates a cursor in the document body', () => {
        expect(cursor).toBeTruthy();
        expect(cursorDot).toBeTruthy();
        expect(cursorRing).toBeTruthy();
    });

    test('Update cursor position when MOVE action received', () => {
        TouchFree.DispatchEvent(
            'TransmitInputAction',
            mockTfInputAction({ CursorPosition: [100, 100], InputType: InputType.MOVE })
        );

        expect(cursorDot?.getAttribute('cx')).toBe('100');
        expect(cursorRing?.getAttribute('cy')).toBe('100');
    });

    test('Cursor ring should grow with ProgressToClick', () => {
        TouchFree.DispatchEvent(
            'TransmitInputAction',
            mockTfInputAction({ InputType: InputType.MOVE, ProgressToClick: 0.1 })
        );

        expect(cursorRing?.getAttribute('r')).toBe('29');
    });
});
