"use strict";
(function webpackUniversalModuleDefinition(root, factory) {
	if(typeof exports === 'object' && typeof module === 'object')
		module.exports = factory();
	else if(typeof define === 'function' && define.amd)
		define([], factory);
	else if(typeof exports === 'object')
		exports["SnappingPlugin"] = factory();
	else
		root["SnappingPlugin"] = factory();
})(self, function() {
return (self["webpackChunktouchfree_tooling_for_web"] = self["webpackChunktouchfree_tooling_for_web"] || []).push([[370],{

/***/ 356:
/***/ ((__unused_webpack_module, exports, __webpack_require__) => {


Object.defineProperty(exports, "__esModule", ({ value: true }));
exports.Ray = void 0;
const Vector2_1 = __webpack_require__(161);
class Ray {
    static Cast(start, end, element) {
        const pos = new Vector2_1.Vector2(start.x, start.y);
        let hasSnap = Ray.Hit(pos, element.getBoundingClientRect());
        let hadSnap = hasSnap;
        // Store the current segment
        const vector = {
            x: end.x - start.x,
            y: end.y - start.y
        };
        // Store the previous evaluated pos
        const prevPos = {
            x: pos.x,
            y: pos.y
        };
        // Store the next end point of the vector
        const nextEnd = {
            x: end.x,
            y: end.y
        };
        // If we already have a snappable under us, it means we're already in a button
        // Let's find the closest edge of the shape by going the other direction
        if (hasSnap) {
            const rect = element.getBoundingClientRect();
            const longest = Math.sqrt(Math.pow(rect.width / 2, 2) +
                Math.pow(rect.height / 2, 2));
            // We'll use the vector as a direction vector, return it, normalize it
            // and multiply by the longest value the shape can have
            vector.x = -vector.x;
            vector.y = -vector.y;
            const vectorLength = Math.sqrt(Math.pow(vector.x, 2) + Math.pow(vector.y, 2));
            vector.x /= vectorLength;
            vector.y /= vectorLength;
            vector.x *= longest;
            vector.y *= longest;
            nextEnd.x = start.x + vector.x;
            nextEnd.y = start.y + vector.y;
        }
        // Store the length of the vector
        let distance = Math.sqrt(Math.pow(vector.x, 2) + Math.pow(vector.y, 2));
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
    static Hit(position, rect) {
        // Is position inside the rect?
        return (position.x < rect.right &&
            position.x > rect.left &&
            position.y < rect.bottom &&
            position.y > rect.top);
    }
}
exports.Ray = Ray;


/***/ }),

/***/ 862:
/***/ ((__unused_webpack_module, exports, __webpack_require__) => {


Object.defineProperty(exports, "__esModule", ({ value: true }));
exports.SnappableElement = void 0;
const Ray_1 = __webpack_require__(356);
const Vector2_1 = __webpack_require__(161);
class SnappableElement {
    constructor(element, distance, closest_point, center, center_distance, hovered) {
        this.element = element;
        this.distance = distance;
        this.closest_point = closest_point;
        this.center = center;
        this.center_distance = center_distance;
        this.hovered = hovered;
    }
    static Compute(element, distant_point) {
        const rect = element.getBoundingClientRect();
        const center = new Vector2_1.Vector2(rect.x + (rect.width / 2), rect.y + (rect.height / 2));
        const center_distance = Math.sqrt(Math.pow(distant_point.x - center.x, 2) +
            Math.pow(distant_point.y - center.y, 2));
        const closest_point = Ray_1.Ray.Cast(distant_point, center, element);
        let distance = Math.sqrt(Math.pow(distant_point.x - closest_point.x, 2) +
            Math.pow(distant_point.y - closest_point.y, 2));
        const hovered = Ray_1.Ray.Hit(distant_point, element.getBoundingClientRect());
        if (hovered) {
            distance = -distance;
        }
        return new SnappableElement(element, distance, closest_point, center, center_distance, hovered);
    }
}
exports.SnappableElement = SnappableElement;


/***/ }),

/***/ 901:
/***/ ((__unused_webpack_module, exports, __webpack_require__) => {


Object.defineProperty(exports, "__esModule", ({ value: true }));
exports.SnappingPlugin = exports.SnapMode = void 0;
const InputActionPlugin_1 = __webpack_require__(988);
const SnappableElement_1 = __webpack_require__(862);
const Vector2_1 = __webpack_require__(161);
var SnapMode;
(function (SnapMode) {
    SnapMode[SnapMode["Magnet"] = 0] = "Magnet";
    SnapMode[SnapMode["Center"] = 1] = "Center";
})(SnapMode = exports.SnapMode || (exports.SnapMode = {}));
class SnappingPlugin extends InputActionPlugin_1.InputActionPlugin {
    constructor() {
        super(...arguments);
        this.snapDistance = 50;
        this.snapMode = SnapMode.Magnet;
        this.snapSoftness = 0.3;
    }
    ModifyInputAction(_inputAction) {
        var _a;
        const cursorPos = {
            x: _inputAction.CursorPosition[0],
            y: _inputAction.CursorPosition[1]
        };
        // Build a list of snappable elements
        let elements = [...document.getElementsByClassName("snappable")].map((value, index, array) => {
            return SnappableElement_1.SnappableElement.Compute(value, Vector2_1.Vector2.FromTuple(cursorPos));
        });
        // Sort them by distance
        elements = elements.sort((a, b) => { return a.distance - b.distance; });
        // Let's snap if there is snappable elements
        if (elements.length > 0) {
            if (elements[0].distance < this.snapDistance) {
                if (this.snapMode === SnapMode.Center) {
                    // If snapForce = 1, cursor position inside the shape is the same
                    // If snapForce = 0, cursor position is snapped in the middle
                    let snapForce = Number.parseFloat((_a = elements[0].element.getAttribute("data-snapforce")) !== null && _a !== void 0 ? _a : this.snapSoftness.toString());
                    // From center of the shape to the border of the shape, following the direction between the center and the cursor
                    // From what we already have, vector = closest_center - closest_point
                    const centerToBorderVector = new Vector2_1.Vector2(elements[0].center.x - elements[0].closest_point.x, elements[0].center.y - elements[0].closest_point.y);
                    const distance = Math.sqrt(Math.pow(centerToBorderVector.x, 2) +
                        Math.pow(centerToBorderVector.y, 2));
                    // Intensity of the lerp between the edge and the center
                    let softSnapT = (elements[0].center_distance / distance) * this.lerp(SnappingPlugin.MIN_SOFTNESS, SnappingPlugin.MAX_SOFTNESS, snapForce);
                    softSnapT = Math.max(Math.min(softSnapT, 1), 0);
                    const finalPos = {
                        x: this.lerp(elements[0].center.x, cursorPos.x, softSnapT),
                        y: this.lerp(elements[0].center.y, cursorPos.y, softSnapT),
                    };
                    _inputAction.CursorPosition[0] = finalPos.x;
                    _inputAction.CursorPosition[1] = finalPos.y;
                }
                else {
                    if (!elements[0].hovered) {
                        _inputAction.CursorPosition[0] = elements[0].closest_point.x;
                        _inputAction.CursorPosition[1] = elements[0].closest_point.y;
                    }
                }
            }
        }
        return _inputAction;
    }
    lerp(x, y, a) {
        return x * (1 - a) + y * a;
    }
    SetSnapModeToMagnet() {
        this.snapMode = SnapMode.Magnet;
    }
    SetSnapModeToCenter() {
        this.snapMode = SnapMode.Center;
    }
    SetSnapDistance(value) {
        this.snapDistance = value;
    }
    SetSnapSoftness(value) {
        this.snapSoftness = value;
    }
}
exports.SnappingPlugin = SnappingPlugin;
SnappingPlugin.MAX_SOFTNESS = 1;
SnappingPlugin.MIN_SOFTNESS = 0;


/***/ }),

/***/ 161:
/***/ ((__unused_webpack_module, exports) => {


Object.defineProperty(exports, "__esModule", ({ value: true }));
exports.Vector2 = void 0;
class Vector2 {
    constructor(x, y) {
        this.x = x;
        this.y = y;
    }
    static FromTuple(tuple) {
        return new Vector2(tuple.x, tuple.y);
    }
    toTuple() {
        return { x: this.x, y: this.y };
    }
}
exports.Vector2 = Vector2;


/***/ })

},
/******/ __webpack_require__ => { // webpackRuntimeModules
/******/ var __webpack_exec__ = (moduleId) => (__webpack_require__(__webpack_require__.s = moduleId))
/******/ var __webpack_exports__ = (__webpack_exec__(901));
/******/ return __webpack_exports__;
/******/ }
]);
});