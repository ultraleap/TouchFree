TouchFree.Connection.ConnectionManager.init();

let controller;
let cursorManager;

window.onload = function () {
    
    cursorManager = new TouchFree.Cursors.CursorManager();
    
    controller = new TouchFree.InputControllers.WebInputController();
}

TouchFree.Connection.ConnectionManager.AddConnectionListener(() => {
    TouchFree.Configuration.ConfigurationManager.RequestConfigChange({UseSwipeInteraction: true}, null, () => {});
});

function togglePageBump() {
    controller.allowBump = !controller.allowBump;
}

function updateBumpDistance(value) {
    controller.bumpDistancePx = value;
}
function updateBumpDuration(value) {
    controller.bumpTotalDurationMS = value;
}

function toggleCursorTextPrompt() {
    cursorManager.cursor.allowTextPrompt = !cursorManager.cursor.allowTextPrompt;
}

function toggleCursorTail() {
    cursorManager.cursor.allowCursorTail = !cursorManager.cursor.allowCursorTail
}