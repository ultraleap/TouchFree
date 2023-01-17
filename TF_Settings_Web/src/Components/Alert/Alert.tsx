import classes from './Alert.module.scss';

import React from 'react';

interface AlertProps {
    show: boolean;
    cssWidth: string;
    text: string;
    animationType: 'fadeIn' | 'fadeInOut' | 'none';
    animationTime: number;
}
const Alert: React.FC<AlertProps> = ({ show, cssWidth, text, animationType, animationTime }) =>
    show ? (
        <div
            className={classes['alert-container']}
            style={{
                width: cssWidth,
                left: `calc(50% - ${cssWidth}/2)`,
                animation: getAlertAnimation(animationType, animationTime),
            }}
        >
            <div className={classes['alert-container--triangle']} />
            <p>{text}</p>
        </div>
    ) : (
        <></>
    );

const getAlertAnimation = (animationType: 'fadeIn' | 'fadeInOut' | 'none', animationTime: number): string => {
    switch (animationType) {
        case 'fadeIn':
            return `fadeInAnimation ease-in ${animationTime}s`;
        case 'fadeInOut':
            return `fadeInOutAnimation ease-in-out ${animationTime}s`;
        case 'none':
        default:
            return '';
    }
};

export default Alert;
