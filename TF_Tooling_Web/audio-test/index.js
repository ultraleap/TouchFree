TouchFree.Connection.ConnectionManager.init();

window.onload = function () {
    new TouchFree.Cursors.SVGCursor();
    new TouchFree.InputControllers.WebInputController();

    TouchFree.Plugins.AudioPlugin.init();
    TouchFree.Plugins.AudioPlugin.instance.loadSound('./audio.wav');
}