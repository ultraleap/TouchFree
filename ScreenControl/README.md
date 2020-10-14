# ScreenControl

The touchless touchscreen system for Unity that handles Ultraleap camera hand tracking input and outputs cursor events. This version was created for Unity 2019.3.3f1 but may work unedited on other versions.

To use in your Unity project:
- Import the ScreenControl .unitypackage into your Unity project.
- Download and import the latest Leap Motion Core Assets Package (4.4.0 or newer) from: https://github.com/leapmotion/UnityModules/releases/

Option 1:
To get started, you may use the 'ScreenControlBase' scene to begin. Once opened:
- Add your content to the 'User Interface Canvas' like you would a normal canvas in Unity.

Option 2:
To use in your own scene:
- Open ScreenControl/Prefabs
- Add the 'ScreenControl' prefab to the scene
- Add the 'LeapHandController' prefab to the scene
- Use an orthographic camera
- Make your own canvas to be interacted with. This should be on the root of the project (not a child of anything else)
    - To trigger Unity Touch Input on your canvas, add the 'UnityUIInputController.cs' script to it. (ScreenControl/Scripts/Interactions/InputControllers/UnityUIInputController.cs)
	- Ensure there is an event system in order to trigger Unity UI events, refeence this in the UnityUIInputController component you created (in both 'Event System' and 'Input Module')
	- Ensure your canvas is not set to Overlay, so users can use the ScreenControl settings screens
		- Set it to 'Screen Space - Camera'
		- Reference the orthographic camera to the canvas' render camera
		- Ensure the 'Plane distance' is viewable by the camera