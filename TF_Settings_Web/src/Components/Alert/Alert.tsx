import './Alert.scss';

import React, { CSSProperties } from 'react';

interface AlertProps {
    show: boolean;
    style: CSSProperties;
    text: string;
    animationType: 'fadeIn' | 'fadeInOut' | 'none';
    animationTime: number;
}
const Alert: React.FC<AlertProps> = ({ show, style, text, animationType, animationTime }) =>
    show ? (
        <div
            className="alert-container"
            style={{
                ...style,
                animation: getAlertAnimation(animationType, animationTime),
            }}
        >
            <div className="alert-container--triangle" />
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
