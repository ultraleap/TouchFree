ScreenControl.Connection.ConnectionManager.init();

window.onload = function () {
    var cursorRing = document.getElementById('cursorRing');
    document.body.appendChild(cursorRing);

    var cursor = document.createElement('img');
    cursor.src = "../images/Dot.png";
    cursor.style.position = "absolute";
    cursor.width = 50;
    cursor.height = 50;
    cursor.style.zIndex = "1001";

    // This is a special class used by the WebInputController to identify the html elements that
    // make up the cursor. This is so it can ignore cursor-related objects when it is looking
    // for elements to pointerover/pointerout etc. Note that the class is also added to the
    // <img> tag found as "cursorRing" above.
    cursor.classList.add('screencontrolcursor');
    document.body.appendChild(cursor);

    var dotCursor = new ScreenControl.Cursors.DotCursor(cursor, cursorRing);
    var inputSystem = new ScreenControl.InputControllers.WebInputController();
}
