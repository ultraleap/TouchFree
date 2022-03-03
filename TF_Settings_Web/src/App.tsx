import React, { CSSProperties } from 'react';
import logo from './logo.svg';

import './App.css';

import { ScreenManager } from './Components/ScreenManager';

declare global {
    interface Window { TouchFree: any; }
}

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
