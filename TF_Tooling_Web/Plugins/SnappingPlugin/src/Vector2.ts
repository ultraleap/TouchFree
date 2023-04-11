export class Vector2 {
    public x: number;
    public y: number;

    constructor(x: number, y: number) {
        this.x = x;
        this.y = y;
    }

    public static FromTuple(tuple: { x: number; y: number }): Vector2 {
        return new Vector2(tuple.x, tuple.y);
    }

    public toTuple(): { x: number; y: number } {
        return { x: this.x, y: this.y };
    }
}
