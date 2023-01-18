import classNames from 'classnames/bind';

import styles from './TFButtons.module.scss';

import React, { CSSProperties } from 'react';

const classes = classNames.bind(styles);

export type TFClickEvent = React.PointerEvent<HTMLButtonElement> | React.KeyboardEvent<HTMLButtonElement>;

export const tFClickIsPointer = (event: TFClickEvent): event is React.PointerEvent<HTMLButtonElement> =>
    event.type.includes('pointer');

export interface BaseButtonProps {
    className?: string;
    buttonStyle?: React.CSSProperties;
    title: string;
    titleStyle?: React.CSSProperties;
    onClick: (event: TFClickEvent) => void;
}

interface TextButtonProps extends BaseButtonProps {
    text: string;
    textStyle?: React.CSSProperties;
    canHover?: boolean;
}

interface IconTextButtonProps extends TextButtonProps {
    icon: string;
    alt: string;
    iconStyle?: React.CSSProperties;
}

export const TextButton: React.FC<TextButtonProps> = ({
    className,
    buttonStyle,
    title,
    titleStyle,
    text,
    textStyle,
    onClick,
    canHover = true,
}) => {
    const content = (
        <>
            <h1 style={titleStyle}>{title}</h1>
            <p style={textStyle}>{text}</p>
        </>
    );

    return BaseTFButton(`${className} text-tf-button`, canHover, onClick, content, buttonStyle);
};

export const VerticalIconTextButton: React.FC<IconTextButtonProps> = ({
    className,
    buttonStyle,
    icon,
    alt,
    iconStyle,
    title,
    titleStyle,
    text,
    textStyle,
    onClick,
    canHover = true,
}) => {
    const content = (
        <>
            <img style={iconStyle} src={icon} alt={alt} />
            <h1 style={titleStyle}>{title}</h1>
            <p style={textStyle}>{text}</p>
        </>
    );

    return BaseTFButton(`${className} vertical-tf-button`, canHover, onClick, content, buttonStyle);
};

export const HorizontalIconTextButton: React.FC<IconTextButtonProps> = ({
    className,
    buttonStyle,
    icon,
    alt,
    iconStyle,
    title,
    titleStyle,
    text,
    textStyle,
    onClick,
    canHover = true,
}) => {
    const content = (
        <>
            <div className={classes('tf-button-text-container')}>
                <h1 style={titleStyle}>{title}</h1>
                <p style={textStyle}>{text}</p>
            </div>
            <img style={iconStyle} src={icon} alt={alt} />
        </>
    );

    return BaseTFButton(`${className} horizontal-tf-button`, canHover, onClick, content, buttonStyle);
};

export const BaseTFButton = (
    buttonClass: string,
    canHover: boolean,
    onClick: (event: TFClickEvent) => void,
    content: JSX.Element,
    buttonStyle?: CSSProperties
) => {
    const [hovered, setHovered] = React.useState<boolean>(false);
    const [pressed, setPressed] = React.useState<boolean>(false);

    return (
        <button
            className={classes(buttonClass, 'tf-button', {
                'tf-button--hovered': canHover && hovered,
                [`tf-button--pressed ${buttonClass}--pressed`]: pressed,
            })}
            style={buttonStyle}
            onPointerOver={() => setHovered(true)}
            onPointerLeave={() => {
                setHovered(false);
                setPressed(false);
            }}
            onPointerDown={() => {
                setPressed(true);
            }}
            onPointerUp={(event) => {
                if (pressed) {
                    onClick(event);
                }
                setPressed(false);
            }}
            onKeyDown={(keyEvent) => {
                if (keyEvent.key === 'Enter') onClick(keyEvent);
            }}
        >
            {content}
        </button>
    );
};
