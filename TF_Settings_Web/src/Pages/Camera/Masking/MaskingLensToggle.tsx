import './CameraMasking.scss';

import React, { useState } from 'react';

import { Lens } from './MaskingScreen';

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
            className={`${isMainLens ? 'lens-toggle--active' : ''} ${
                hovered ? 'lens-toggle--hovered' : ''
            } lens-toggle--${lens.toLowerCase()}`}
        >
            <p>{lens} Lens</p>
        </div>
    );
};

export default MaskingLensToggle;
