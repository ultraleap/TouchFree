import * as test_example from "../src/test_example";

test('invert function', () => {
    expect(test_example.invert(true)).toBe(false);
});