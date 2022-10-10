TouchFree.Connection.ConnectionManager.init();

window.onload = function () {
    window.cursorManager = new TouchFree.Cursors.CursorManager({
      spriteSheetUrl: "./images/cursor_sprites.png",
      blankUrl: "./images/blank.png",
    });
    cursorManager.SetActiveCursor('hand');
    new TouchFree.InputControllers.WebInputController();
}

const setCursor = (cursor) => {
    cursorManager.SetActiveCursor(cursor);
}