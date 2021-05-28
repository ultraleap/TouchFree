ScreenControl.Connection.ConnectionManager.init();

window.onload = function () {
    var cursorRing = document.createElement('img');
    cursorRing.src = "./images/Ring.png"
    cursorRing.style.position = "absolute";
    cursorRing.width = 30;
    cursorRing.height = 30;
    cursorRing.style.zIndex = "1000";

    var cursor = document.createElement('img');
    cursor.src = "./images/Dot.png";
    cursor.style.position = "absolute";
    cursor.width = 30;
    cursor.height = 30;
    cursor.style.zIndex = "1001";

    // This is a special class used by the WebInputController to identify the html elements that
    // make up the cursor. This is so it can ignore cursor-related objects when it is looking
    // for elements to pointerover/pointerout etc.
    cursor.classList.add('screencontrolcursor');
    cursorRing.classList.add('screencontrolcursor');

    document.body.appendChild(cursor);
    document.body.appendChild(cursorRing);

    var dotCursor = new ScreenControl.Cursors.DotCursor(cursor, cursorRing);
    dotCursor.distanceSnap = 100;
    var inputSystem = new ScreenControl.InputControllers.WebInputController();
    inputSystem.distanceSnap = 100;
}