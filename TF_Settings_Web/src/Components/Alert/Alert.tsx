import styles from './Alert.module.scss';

import classnames from 'classnames/bind';
import React, { CSSProperties } from 'react';

const classes = classnames.bind(styles);

interface AlertProps {
    show: boolean;
    style: CSSProperties;
    text: string;
    animationType: 'fadeIn' | 'fadeInOut' | 'none';
    animationTime: number;
}
const Alert: React.FC<AlertProps> = ({ show, style, text, animationType, animationTime }) => {
    if (!show) return <></>;

    return (
        <div
            className={classes('alert-container')}
            style={{
                ...style,
                animation: getAlertAnimation(animationType, animationTime),
            }}
        >
            <div className={classes('alert-container--triangle')} />
            <p>{text}</p>
        </div>
    );
};

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
