import './TFButtons.scss';

import React, { CSSProperties } from 'react';

export type TFClickEvent = React.PointerEvent<HTMLButtonElement> | React.KeyboardEvent<HTMLButtonElement>;

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

const getButtonTextElements = (
    title: string,
    text: string,
    titleStyle?: CSSProperties,
    textStyle?: CSSProperties
): JSX.Element[] => {
    const elements: JSX.Element[] = [];
    if (title) {
        elements.push(
            <h1 key={title} style={titleStyle}>
                {title}
            </h1>
        );
    }

    if (text) {
        elements.push(
            <p key={text} style={textStyle}>
                {text}
            </p>
        );
    }

    return elements;
};

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
    const content = <>{getButtonTextElements(title, text, titleStyle, textStyle)}</>;

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
            {getButtonTextElements(title, text, titleStyle, textStyle)}
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
            <div className="tf-button-text-container">{getButtonTextElements(title, text, titleStyle, textStyle)}</div>
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
            className={`${buttonClass} tf-button ${
                canHover && hovered ? `tf-button--hovered ${buttonClass}--hovered` : ''
            } ${pressed ? `tf-button--pressed ${buttonClass}--pressed` : ''}`}
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
