import 'Styles/Controls/TFButton.scss';

import React, { CSSProperties } from 'react';

import { TFClickEvent } from 'Components/SettingsTypes';

interface TextButtonProps {
    buttonStyle: React.CSSProperties;
    title: string;
    titleStyle?: React.CSSProperties;
    text: string;
    textStyle?: React.CSSProperties;
    onClick: (event: React.PointerEvent<HTMLButtonElement> | React.KeyboardEvent<HTMLButtonElement>) => void;
    canHover?: boolean;
}

interface IconTextButtonProps extends TextButtonProps {
    buttonStyle: React.CSSProperties;
    icon: string;
    alt: string;
    iconStyle?: React.CSSProperties;
}

export const TextButton: React.FC<TextButtonProps> = ({
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

    return TFButton('text-tf-button', canHover, buttonStyle, onClick, content);
};

export const VerticalIconTextButton: React.FC<IconTextButtonProps> = ({
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

    return TFButton('vertical-tf-button', canHover, buttonStyle, onClick, content);
};

export const HorizontalIconTextButton: React.FC<IconTextButtonProps> = ({
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
            <div className="tf-button-text-container">
                <h1 style={titleStyle}>{title}</h1>
                <p style={textStyle}>{text}</p>
            </div>
            <img style={iconStyle} src={icon} alt={alt} />
        </>
    );

    return TFButton('horizontal-tf-button', canHover, buttonStyle, onClick, content);
};

const TFButton = (
    buttonClass: string,
    canHover: boolean,
    buttonStyle: CSSProperties,
    onClick: (event: TFClickEvent) => void,
    content: JSX.Element
) => {
    const [hovered, setHovered] = React.useState<boolean>(false);
    const [pressed, setPressed] = React.useState<boolean>(false);

    return (
        <button
            className={`${buttonClass} tf-button ${canHover && hovered ? 'tf-button--hovered' : ''} ${
                pressed ? 'tf-button--pressed' : ''
            }`}
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
