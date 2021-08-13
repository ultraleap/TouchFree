import { Vector2 } from './Vector2';

export class Ray {
    public static Cast(start: Vector2, end: Vector2, element: Element): Vector2 {
        const pos: Vector2 = new Vector2(start.x, start.y);

        let hasSnap: boolean = Ray.Hit(pos, element);
        let hadSnap: boolean = hasSnap;

        // Store the current segment
        const vector: { x: number, y: number } = {
            x: end.x - start.x,
            y: end.y - start.y
        };

        // Store the previous evaluated pos
        const prevPos: { x: number, y: number } = {
            x: pos.x,
            y: pos.y
        }

        // Store the next end point of the vector
        const nextEnd: { x: number, y: number } = {
            x: end.x,
            y: end.y
        }

        // If we already have a snappable under us, it means we're already in a button
        // Let's find the closest edge of the shape by going the other direction
        if (hasSnap) {
            const rect: DOMRect = element.getBoundingClientRect();
            const longest: number = Math.sqrt(
                Math.pow(rect.width / 2, 2) +
                Math.pow(rect.height / 2, 2)
            );

            // We'll use the vector as a direction vector, return it, normalize it
            // and multiply by the longest value the shape can have
            vector.x = -vector.x;
            vector.y = -vector.y;

            const vectorLength: number = Math.sqrt(Math.pow(vector.x, 2) + Math.pow(vector.y, 2));

            vector.x /= vectorLength;
            vector.y /= vectorLength;

            vector.x *= longest;
            vector.y *= longest;

            nextEnd.x = start.x + vector.x;
            nextEnd.y = start.y + vector.y;
        }

        // Store the length of the vector
        let distance: number = Math.sqrt(Math.pow(vector.x, 2) + Math.pow(vector.y, 2));

        // We already checked the starting pos, let's step in the first step
        pos.x = pos.x + (vector.x / 2);
        pos.y = pos.y + (vector.y / 2);

        while (distance > 1) {
            hasSnap = Ray.Hit(pos, element.getBoundingClientRect());

            // If we changed state, we reverse the direction
            if ((hasSnap && !hadSnap) || (!hasSnap && hadSnap)) {
                nextEnd.x = prevPos.x;
                nextEnd.y = prevPos.y;
            }

            // We get the new vector along we're moving
            vector.x = nextEnd.x - pos.x;
            vector.y = nextEnd.y - pos.y;

            // We get the new vector length
            distance = Math.sqrt(Math.pow(vector.x, 2) + Math.pow(vector.y, 2));

            // We store the previous position
            prevPos.x = pos.x;
            prevPos.y = pos.y;

            // We get the next position
            pos.x = pos.x + (vector.x / 2);
            pos.y = pos.y + (vector.y / 2);

            hadSnap = hasSnap;
        }

        return pos;
    }

    public static Hit(position: Vector2, rect: DOMRect): boolean {

        return !(rect.right < position.x ||
            rect.left > position.x ||
            rect.bottom < position.y ||
            rect.top > position.y);
    }
}