import 'Styles/Camera/CameraMasking.scss';

import React, { useEffect, useRef, useState } from 'react';
import { CSSTransition } from 'react-transition-group';

import { ToggleSwitch } from 'Components/Controls/ToggleSwitch';

interface MaskingOptionProps {
    title: string;
    description: string;
    value: boolean;
    onChange: (value: boolean) => void;
    isMouseOnly?: boolean;
}

const MaskingOption: React.FC<MaskingOptionProps> = ({ title, description, value, onChange, isMouseOnly }) => {
    const [showMousePrompt, setShowMousePrompt] = useState<boolean>(false);

    const timeoutRef = useRef<number>();

    useEffect(() => {
        return () => {
            window.clearTimeout(timeoutRef.current);
        };
    }, []);

    return (
        <label
            className="cam-feeds-option"
            onPointerDown={(event) => {
                if (isMouseOnly && event.pointerType === 'pen') {
                    setShowMousePrompt(true);
                    window.clearTimeout(timeoutRef.current);
                    timeoutRef.current = window.setTimeout(() => setShowMousePrompt(false), 3000);
                } else {
                    onChange(!value);
                }
            }}
        >
            <div className="cam-feeds-option-text">
                <h1>{title}</h1>
                <p>{description}</p>
            </div>
            <div className="cam-feeds-option-toggle">
                <ToggleSwitch value={value} />
            </div>
            <CSSTransition in={showMousePrompt} timeout={500} classNames="fade" unmountOnExit>
                <MousePrompt />
            </CSSTransition>
        </label>
    );
};

const MousePrompt: React.FC = () => (
    <div className="cam-feeds-option--no-TF" onPointerDown={(e) => e.stopPropagation()}>
        <div className="cam-feeds-option--no-TF--triangle" />
        <p>Cannot be selected using the TouchFree cursor</p>
    </div>
);

export default MaskingOption;
