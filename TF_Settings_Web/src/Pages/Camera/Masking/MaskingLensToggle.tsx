import styles from './CameraMasking.module.scss';

import classnames from 'classnames/bind';
import React, { useState } from 'react';

import { Lens } from './MaskingScreen';

const classes = classnames.bind(styles);

interface MaskingLensToggle {
    lens: Lens;
    isMainLens: boolean;
    setMainLens: (value: Lens) => void;
}

const MaskingLensToggle: React.FC<MaskingLensToggle> = ({ lens, isMainLens, setMainLens }) => {
    const [hovered, setHovered] = useState<boolean>(false);

    return (
        <div
            onPointerDown={() => setMainLens(lens)}
            onPointerEnter={() => setHovered(true)}
            onPointerLeave={() => setHovered(false)}
            className={classes(`lens-toggle--${lens.toLowerCase()}`, {
                'lens-toggle--active': isMainLens,
                'lens-toggle--hovered': hovered,
            })}
        >
            <p>{lens} Lens</p>
        </div>
    );
};

export default MaskingLensToggle;
