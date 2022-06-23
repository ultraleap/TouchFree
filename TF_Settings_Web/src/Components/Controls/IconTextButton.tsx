import React from 'react';

import '../../Styles/Controls/IconTextButton.css';

interface IconTextButtonProps {
    buttonStyle: React.CSSProperties;
    icon: string;
    iconStyle: React.CSSProperties;
    title: string;
    titleStyle?: React.CSSProperties;
    text: string;
    textStyle?: React.CSSProperties;
    onClick: () => void;
}

const IconTextButton: React.FC<IconTextButtonProps> = ({
    buttonStyle,
    icon,
    iconStyle,
    title,
    titleStyle,
    text,
    textStyle,
    onClick,
}) => {
    const [hovered, setHovered] = React.useState<boolean>(false);
    const [pressed, setPressed] = React.useState<boolean>(false);

    return (
        <button
            className={`IconTextButton ${hovered ? ' IconTextButtonHovered' : ''} ${
                pressed ? ' IconTextButtonPressed' : ''
            }`}
            style={buttonStyle}
            onPointerOver={() => setHovered(true)}
            // onPointerEnter={() => setHovered(true)}
            onPointerLeave={() => {
                setHovered(false);
                setPressed(false);
            }}
            onPointerDown={() => setPressed(true)}
            onPointerUp={() => {
                setPressed(false);
                onClick();
            }}
        >
            <img src={icon} style={iconStyle} />
            <h1 style={titleStyle}>{title}</h1>
            <p style={textStyle}>{text}</p>
        </button>
    );
};

export default IconTextButton;
