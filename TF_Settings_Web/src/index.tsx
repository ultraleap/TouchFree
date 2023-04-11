import App from '@/App';

import '@/Fonts/fonts.css';
import '@/index.scss';

import ReactDOM from 'react-dom';
import { BrowserRouter } from 'react-router-dom';

// If the app is reloaded manually, it should return to '/settings/index.html'
const nav = performance.getEntriesByType('navigation')[0] as PerformanceNavigationTiming;
if (['navigate', 'reload'].includes(nav.type) && !window.location.href.endsWith('/settings/index.html')) {
    window.location.href = '/settings/index.html';
}
ReactDOM.render(
    <BrowserRouter>
        <App />
    </BrowserRouter>,
    document.querySelector('#root')
);
