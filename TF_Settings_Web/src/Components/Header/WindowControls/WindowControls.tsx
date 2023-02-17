import styles from './WindowControls.module.scss';

import classnames from 'classnames/bind';
import React, { useState } from 'react';

import { closeWindow, minimizeWindow, toggleFullScreen } from '@/TauriUtils';

import {
    WindowCloseIcon,
    WindowFullscreenIcon,
    WindowMinusIcon,
    WindowMinusIconGlow,
    WindowFullscreenIconGlow,
    WindowCloseIconGlow,
} from '@/Images';

const classes = classnames.bind(styles);

const WindowControls: React.FC = () => {
    return (
        <div className={classes('window-controls')}>
            <WindowControl
                imagePath={WindowMinusIcon}
                hoverImagePath={WindowMinusIconGlow}
                onClick={() => minimizeWindow()}
            />
            <WindowControl
                imagePath={WindowFullscreenIcon}
                hoverImagePath={WindowFullscreenIconGlow}
                onClick={() => toggleFullScreen()}
            />
            <WindowControl
                imagePath={WindowCloseIcon}
                hoverImagePath={WindowCloseIconGlow}
                onClick={() => closeWindow()}
            />
        </div>
    );
};

interface WindowControlProps {
    imagePath: string;
    hoverImagePath?: string;
    onClick: () => void;
}

const WindowControl: React.FC<WindowControlProps> = ({ imagePath, hoverImagePath, onClick }) => {
    const [hovered, setHovered] = useState<boolean>(false);

    return (
        <div
            className={classes('window-controls__control')}
            onPointerEnter={() => setHovered(true)}
            onPointerLeave={() => setHovered(false)}
            onClick={onClick}
        >
            <img src={hoverImagePath && hovered ? hoverImagePath : imagePath} />
        </div>
    );
};

export default WindowControls;
