// Function: MapRangeToRange
// Map _value from a range of _oldMin to _oldMax to a new range of _newMin to _newMax.
//
// e.g. the result of MapRangeToRange(0.5, 0, 1, 0, 8) is 4.
export function MapRangeToRange(
    _value: number,
    _oldMin: number,
    _oldMax: number,
    _newMin: number,
    _newMax: number
): number {
    const oldRange = _oldMax - _oldMin;
    let newValue;

    if (oldRange === 0) {
        newValue = _newMin;
    } else {
        const newRange = _newMax - _newMin;
        newValue = ((_value - _oldMin) * newRange) / oldRange + _newMin;
    }

    return newValue;
}
