# Hand Cursor

The Hand Cursor provides an articulated hand model that responds as a cursor.
The hand model is placed at a cursor location and can be scaled to suit your
UI.

##  How to use

### Setting up the Hand

* Add the Hand Cursor prefab to your scene.
* Connect the camera used for your UI to the `Cursor Camera` property of the `LeapHandCursor` component
* Connect the `LeapHandController` from your scene to the `InputLeapProvider` property of the `HandCursorPostProcessor`

### Setting up your UI

* The hand cursor can be projected into your UI by providing colliders for the UI elements
* The hand raycasts into the scene using the Physics.Raycast
