import styles from './CameraMasking.module.scss';

import classnames from 'classnames/bind';
import React, { useEffect, useRef, useState } from 'react';

import { Alert, ToggleSwitch } from '@/Components';

const classes = classnames.bind(styles);

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
            className={classes('cam-feeds-option')}
            onPointerDown={(event) => {
                if (isMouseOnly && event.pointerType === 'pen') {
                    if (!showMousePrompt) {
                        setShowMousePrompt(true);
                        timeoutRef.current = window.setTimeout(() => setShowMousePrompt(false), 4000);
                    }
                } else {
                    onChange(!value);
                }
            }}
        >
            <div className={classes('cam-feeds-option-text')}>
                <h1>{title}</h1>
                <p>{description}</p>
            </div>
            <div className={classes('cam-feeds-option-toggle')}>
                <ToggleSwitch value={value} />
            </div>
            <Alert
                show={showMousePrompt}
                style={{ width: '100%', bottom: '-70px' }}
                text="Cannot be selected using the TouchFree cursor"
                animationType="fadeInOut"
                animationTime={4}
            />
        </label>
    );
};

export default MaskingOption;
