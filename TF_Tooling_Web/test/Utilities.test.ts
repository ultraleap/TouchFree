import { MapRangeToRange } from '../src/Utilities';

describe('MapRangeToRange', () => {
    test('Map value in range to same range gives same value', () => {
        const range = [0, 100];
        const value = 1;
        expect(MapRangeToRange(value, range[0], range[1], range[0], range[1])).toBe(value);
    });

    test('Map value in range to new range gives expected value in range', () => {
        const originalRange = [0, 100];
        const value = 1;
        const newRange = [0, 1000];
        const newValue = 10;
        expect(MapRangeToRange(value, originalRange[0], originalRange[1], newRange[0], newRange[1])).toBe(newValue);
    });

    test('Old range has zero size returns new minimum value', () => {
        const originalRange = [0, 0];
        const value = 1;
        const newRange = [23, 45];
        expect(MapRangeToRange(value, originalRange[0], originalRange[1], newRange[0], newRange[1])).toBe(newRange[0]);
    });

    test('Old range has largest value first translates with range limits as given', () => {
        const originalRange = [100, 0];
        const value = 1;
        const newRange = [0, 100];
        const newValue = 99;
        expect(MapRangeToRange(value, originalRange[0], originalRange[1], newRange[0], newRange[1])).toBe(newValue);
    });

    test('New range has largest value first translates with range limits as given', () => {
        const originalRange = [0, 100];
        const value = 1;
        const newRange = [100, 0];
        const newValue = 99;
        expect(MapRangeToRange(value, originalRange[0], originalRange[1], newRange[0], newRange[1])).toBe(newValue);
    });
});
