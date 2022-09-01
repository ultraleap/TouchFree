TouchFree.Connection.ConnectionManager.init();

window.onload = function () {
    
    new TouchFree.Cursors.CursorManager();
    
    new TouchFree.InputControllers.WebInputController();
}

TouchFree.Connection.ConnectionManager.AddConnectionListener(() => {
    TouchFree.Configuration.ConfigurationManager.RequestConfigChange({UseSwipeInteraction: true}, null, () => {});
});