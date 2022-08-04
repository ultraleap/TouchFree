import 'Styles/Camera/CameraMasking.scss';

import React from 'react';

import { Lens } from './MaskingScreen';

interface MaskingLensToggle {
    lens: Lens;
    isMainLens: boolean;
    setMainLens: (value: Lens) => void;
}

const MaskingLensToggle: React.FC<MaskingLensToggle> = ({ lens, isMainLens, setMainLens }) => (
    <div
        onPointerDown={() => setMainLens(lens)}
        className={`${isMainLens ? 'lens-toggle--active' : ''} lens-toggle--${lens.toLowerCase()}`}
    >
        {lens} Lens
    </div>
);

export default MaskingLensToggle;
