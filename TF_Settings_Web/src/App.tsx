import React, { CSSProperties } from 'react';
import logo from './logo.svg';

import './App.css';

import { ScreenManager } from './Components/ScreenManager';

// Load tooling here & use throughout

// <ControlBar/>
// <ScreenManager>
    // Literally just put a blue box here called ManualSetup
    // Literally just put a red box here called InteractionSetup (background?)
// </ScreenManager>

class App extends React.Component {
    private containerStyle : CSSProperties = {
        display: 'flex',
        flexDirection: 'column',

        height: '100%'
    }

    render() {
        return (
            <div className="App" style={this.containerStyle}>
                <ScreenManager/>
            </div>
        );
    }
}

export default App;
