import './Fonts/fonts.css';
import './index.css';

import ReactDOM from 'react-dom';
import { BrowserRouter } from 'react-router-dom';

import App from './App';

export const APP_WIDTH = getComputedStyle(document.documentElement).getPropertyValue('--app-width');
export const APP_MARGIN_LEFT = getComputedStyle(document.documentElement).getPropertyValue('--app-margin-left');
export const ULTRALEAP_GREEN = getComputedStyle(document.documentElement).getPropertyValue('--ultraleap-green');

ReactDOM.render(
    <BrowserRouter>
        <App />
    </BrowserRouter>,
    document.querySelector('#root')
);
