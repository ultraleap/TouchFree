//=================================================//
//            Setting up TouchFree                 //
//=================================================//

// This initialises the connection to TouchFree
TouchFree.Connection.ConnectionManager.init();

window.onload = () => {
    // This adds a cursor to the page so that you can see what you're doing
    var svgCursor = new TouchFree.Cursors.SVGCursor();

    // This sets up sending handling the events sent by TouchFree so that the cursor
    // updates and clicks on elements. You can use this to add custom handling of
    // cursor movement.
    var inputSystem = new TouchFree.InputControllers.WebInputController();
}

//=================================================//
//            Web Button click handling            //
//=================================================//

// Handle button clicks
var changeText = function (content) {
    var textToChange = document.getElementById("Text to change");
    textToChange.textContent = content;
};

var resetText = function () {
    changeText("");
}

var resetTextColor = function () {
    var textToChange = document.getElementById("Text to change");
    textToChange.style.color = "black";
}

var textColor = function (content) {
    var textToChange = document.getElementById("Text to change");
    textToChange.style.color = content;
}

// Handle button hovering
var buttonPointerEntered = (button) => {
    var buttonClass = '';
    button.classList.forEach(b => {
        if (b.startsWith('button') && !b.endsWith('-hovered')) {
            buttonClass = b;
        }
    });
    if (buttonClass !== '') {
        button.classList.add(buttonClass + '-hovered');
    }
}

var buttonPointerLeave = (button) => {
    var buttonClass = '';
    button.classList.forEach(b => {
        if (b.startsWith('button') && !b.endsWith('-hovered')) {
            buttonClass = b;
        }
    });
    if (button.classList.contains(buttonClass + '-hovered')) {
        button.classList.remove(buttonClass + '-hovered');
    }
}