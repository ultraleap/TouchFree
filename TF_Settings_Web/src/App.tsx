import React, { CSSProperties, RefObject } from 'react';

import { ConnectionManager } from "./TouchFree/Connection/ConnectionManager";
import { BaseInputController } from "./TouchFree/InputControllers/BaseInputController";
import { WebInputController } from "./TouchFree/InputControllers/WebInputController";

import { CursorManager } from "./Components/CursorManager";
import { ScreenManager } from './Components/ScreenManager';

import './App.css';
import './Styles/Styles.css';

declare global {
    interface Window { TouchFree: any; }
}

class App extends React.Component {
    private containerStyle : CSSProperties = {
        display: 'flex',
        flexDirection: 'column',

        height: '100%'
    }

    private cursorManager: CursorManager;
    private cursorParent: RefObject<HTMLDivElement>;

    // TouchFree objects
    private inputSystem: BaseInputController;

    constructor(props: {}) {
        super(props);
        ConnectionManager.init();
        this.inputSystem = new WebInputController();

        this.cursorManager = new CursorManager();
        this.cursorParent = React.createRef();

        console.log("App Constructor called");
    }

    componentDidMount() {
        if (this.cursorParent.current !== null)
        {
            this.cursorManager.setElement(this.cursorParent.current);
        }
    }

    componentWillUnmount() {
        this.inputSystem.disconnect();
    }


    render() {
        return (
            <div className="App" style={this.containerStyle}
                ref={this.cursorParent}>
                <ScreenManager/>
            </div>
        );
    }
}

export default App;
