import { Ray } from './Ray'
import { Vector2 } from './Vector2';

export class SnappableElement {
    public element: Element;
    public distance: number;
    public closest_point: Vector2;
    public center: Vector2;
    public center_distance: number;
    public hovered: boolean;

    constructor(
        element: Element,
        distance: number,
        closest_point: Vector2,
        center: Vector2,
        center_distance: number,
        hovered: boolean
    ) {
        this.element = element;
        this.distance = distance;
        this.closest_point = closest_point;
        this.center = center;
        this.center_distance = center_distance;
        this.hovered = hovered;
    }

    public static Compute(element: Element, distant_point: Vector2): SnappableElement {
        const rect: DOMRect = element.getBoundingClientRect();
        const center: Vector2 = new Vector2(rect.left + (rect.width / 2), rect.top + (rect.height / 2));
        const center_distance: number = Math.sqrt(
            Math.pow(distant_point.x - center.x, 2) +
            Math.pow(distant_point.y - center.y, 2)
        );

        const closest_point: Vector2 = Ray.Cast(distant_point, center, element);
        let distance: number = Math.sqrt(
            Math.pow(distant_point.x - closest_point.x, 2) +
            Math.pow(distant_point.y - closest_point.y, 2)
        );

        const hovered: boolean = Ray.Hit(distant_point, element.getBoundingClientRect());
        if (hovered) {
            distance = -distance;
        }

        return new SnappableElement(element, distance, closest_point, center, center_distance, hovered);
    }
}