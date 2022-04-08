import React, { CSSProperties, RefObject } from 'react';

import { ConnectionManager } from "./TouchFree/Connection/ConnectionManager";
import { BaseInputController } from "./TouchFree/InputControllers/BaseInputController";
import { WebInputController } from "./TouchFree/InputControllers/WebInputController";

import { CursorManager } from "./Components/CursorManager";
import { ScreenManager } from './Components/ScreenManager';

import './App.css';
import './Styles/Styles.css';
import { InputActionManager } from './TouchFree/Plugins/InputActionManager';
import { InputActionPlugin } from './TouchFree/Plugins/InputActionPlugin';
import { InputPlugin } from './Components/InputPlugin';

declare global {
    interface Window { TouchFree: any; }
}

class App extends React.Component {
    private containerStyle : CSSProperties = {
        top: '0px',
        bottom: '0px',
        position: 'absolute',
        height: '100%',
        width: '1080px',
        backgroundColor: '#222222',
    }

    private cursorManager: CursorManager;
    private cursorParent: RefObject<HTMLDivElement>;

    // TouchFree objects
    private inputPlugins: Array<InputActionPlugin>;
    // private inputSystem: BaseInputController;

    constructor(props: {}) {
        super(props);
        ConnectionManager.init();

        this.inputPlugins = [
            new InputPlugin()
        ];
        InputActionManager.SetPlugins(this.inputPlugins);

        // this.inputSystem = new WebInputController();

        this.cursorManager = new CursorManager();
        this.cursorParent = React.createRef();
    }

    componentDidMount() {
        if (this.cursorParent.current !== null)
        {
            this.cursorManager.setElement(this.cursorParent.current);
        }
    }

    componentWillUnmount() {
        // this.inputSystem.disconnect();
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
