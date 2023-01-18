import classNames from 'classnames/bind';

import styles from './TFButtons.module.scss';

import React, { ReactNode } from 'react';

const classes = classNames.bind(styles);

export type TFClickEvent = React.PointerEvent<HTMLButtonElement> | React.KeyboardEvent<HTMLButtonElement>;

export const tFClickIsPointer = (event: TFClickEvent): event is React.PointerEvent<HTMLButtonElement> =>
    event.type.includes('pointer');

interface BaseButtonProps {
    className?: string;
    buttonStyle?: React.CSSProperties;
    titleStyle?: React.CSSProperties;
    onClick: (event: TFClickEvent) => void;
    canHover?: boolean;
}

interface TextButtonProps extends BaseButtonProps {
    text: string;
    title: string;
    textStyle?: React.CSSProperties;
}

interface IconTextButtonProps extends TextButtonProps {
    icon: string;
    alt: string;
    title: string;
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
}) => (
    <BaseTFButton
        buttonClass={className}
        canHover={canHover}
        onClick={onClick}
        buttonStyle={buttonStyle}
        type="vertical"
    >
        <h1 style={titleStyle}>{title}</h1>
        <p style={textStyle}>{text}</p>
    </BaseTFButton>
);

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
}) => (
    <BaseTFButton
        buttonClass={className}
        canHover={canHover}
        onClick={onClick}
        buttonStyle={buttonStyle}
        type="vertical"
    >
        <img style={iconStyle} src={icon} alt={alt} />
        <h1 style={titleStyle}>{title}</h1>
        <p style={textStyle}>{text}</p>
    </BaseTFButton>
);

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
}) => (
    <BaseTFButton
        buttonClass={className}
        canHover={canHover}
        onClick={onClick}
        buttonStyle={buttonStyle}
        type="horizontal"
    >
        <div className={classes('tf-button-text-container')}>
            <h1 style={titleStyle}>{title}</h1>
            <p style={textStyle}>{text}</p>
        </div>
        <img style={iconStyle} src={icon} alt={alt} />
    </BaseTFButton>
);

interface MiscTextButtonProps extends BaseButtonProps {
    title: string;
}

export const MiscTextButton: React.FC<MiscTextButtonProps> = ({ buttonStyle, title, titleStyle, onClick }) => {
    return (
        <BaseTFButton canHover onClick={onClick} buttonStyle={buttonStyle} type="misc">
            <h1 style={titleStyle}>{title}</h1>
        </BaseTFButton>
    );
};

interface BaseTFButtonProps extends BaseButtonProps {
    type: 'horizontal' | 'vertical' | 'text' | 'misc';
    buttonClass?: string;
    children?: ReactNode;
}

export const BaseTFButton: React.FC<BaseTFButtonProps> = ({
    type,
    buttonClass,
    canHover,
    onClick,
    buttonStyle,
    children,
}) => {
    const [hovered, setHovered] = React.useState<boolean>(false);
    const [pressed, setPressed] = React.useState<boolean>(false);

    return (
        <button
            className={classes(buttonClass, 'tf-button', `tf-button--${type}`, {
                'tf-button--hovered': canHover && hovered,
                [`tf-button--${type}--hovered`]: canHover && hovered,
                'tf-button--pressed': pressed,
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
            {children}
        </button>
    );
};
